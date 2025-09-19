using CvjmRechnung.Interfaces;
using CvjmRechnung.Services;
using CvjmRechnung.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CvjmRechnung
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        public App()
        {
            _logger.Info("***** PROGRAM STARTED *****");
            Services = ConfigureServices();

            this.InitializeComponent();
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
