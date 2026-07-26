using CvjmRechnung.Interfaces;
using CvjmRechnung.Services;
using CvjmRechnung.ViewModel;
using Microsoft.Extensions.DependencyInjection;
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
            VelopackApp.Build().Run();

            var app = new App();
            app.InitializeComponent();
            var splashScreen = new SplashScreen("Resources/logo.png");
            splashScreen.Show(autoClose: false);

            try
            {
                app.CheckForUpdatesAsync().GetAwaiter().GetResult();
            }
            finally
            {
                splashScreen.Close(TimeSpan.Zero);
            }

            app.Run();
        }

        public App()
        {
            _logger.Info("***** PROGRAM STARTED *****");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Services = ConfigureServices();
            // Subscribe to unobserved task exceptions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private async Task CheckForUpdatesAsync()
        {
            var updateManager = new UpdateManager(new GithubSource(
                repoUrl: GetRepositoryUrl(),
                accessToken: string.Empty,
                prerelease: false,
                downloader: null));

            var update = await updateManager.CheckForUpdatesAsync();
            if (update != null)
            {
                await updateManager.DownloadUpdatesAsync(update);
                updateManager.ApplyUpdatesAndRestart(update);
            }
        }

        private static string GetRepositoryUrl() => "https://github.com/HugeGo88/CvjmRechnung";

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
