using CvjmRechnung.Interfaces;
using CvjmRechnung.Services;
using CvjmRechnung.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Windows;
using Velopack;
using Velopack.Sources;

namespace CvjmRechnung
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                _logger.Info("***** PROGRAM STARTED *****");

                VelopackApp.Build().Run();

                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Fatal exception in application entry point");
                MessageBox.Show($"Fatal Error: {ex.Message}\nApplication is shutting down.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Services = ConfigureServices();
            // Subscribe to unobserved task exceptions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.Error(e.Exception, "Unobserved task exception occurred");
            // Log the exception (e.Exception)
            MessageBox.Show($"Background Task Error: {e.Exception.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

            // Mark the exception as observed to prevent the process from crashing
            e.SetObserved();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Subscribe to the DispatcherUnhandledException event
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);
            StartUpdateCheck();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var updateManager = new UpdateManager(new GithubSource(
                    repoUrl: GetRepositoryUrl(),
                    accessToken: string.Empty,
                    prerelease: false,
                    downloader: null));
                if (!updateManager.IsInstalled)
                {
                    _logger.Info("Skipping update check because the app is not installed from a packaged release.");
                    return;
                }

                var update = await updateManager.CheckForUpdatesAsync();
                if (update is null)
                {
                    _logger.Info("No application update available.");
                    return;
                }

                var result = MessageBox.Show(
                    $"Version {update.TargetFullRelease.Version} ist verfügbar. Möchtest du das Update jetzt herunterladen und die App neu starten?",
                    "Update verfügbar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result != MessageBoxResult.Yes)
                {
                    _logger.Info("User skipped the available update.");
                    return;
                }

                await updateManager.DownloadUpdatesAsync(update);
                updateManager.ApplyUpdatesAndRestart(update);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to check for updates.");
            }
        }

        private void StartUpdateCheck()
        {
            _ = CheckForUpdatesAsync().ContinueWith(task =>
            {
                _logger.Warn(task.Exception, "Unexpected failure while checking for updates.");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private static string GetRepositoryUrl()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(attribute => attribute.Key == "RepositoryUrl")
                ?.Value
                ?? "https://github.com/HugeGo88/CvjmRechnung";
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Error(e.Exception, "Unhandled UI exception occurred");

            // Log the exception details
            MessageBox.Show($"UI Error: {e.Exception.Message}\nApplication will attempt to continue.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

            // Optionally prevent the crash. Use with caution!
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Get the exception object
            Exception ex = e.ExceptionObject as Exception;

            _logger.Error(ex, "Unhandled exception occurred");

            // Log the exception details and inform the user
            MessageBox.Show($"Fatal Error (AppDomain): {ex?.Message}\nApplication is shutting down.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Optionally, perform cleanup or logging before the application terminates
            // ...
        }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            // Add Services
            services.AddSingleton<IMailClient, MailClientService>();
            services.AddSingleton<IConfiguration, ConfigurationService>();
            // Add ViewModels
            services.AddTransient<MainWindowViewModel>();
            return services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                App.Current.Services.GetService<IConfiguration>()?.Save();
                _logger.Info("***** PROGRAM ENDED *****");

            }
            finally
            {
                base.OnExit(e);
            }
        }
    }

}
