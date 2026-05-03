using CvjmRechnung.Interfaces;
using System.Windows;

namespace CvjmRechnung.View
{
    public partial class SettingsWindow : Window
    {
        private readonly IConfiguration _configuration;

        public SettingsWindow(IConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            PasswordBox.Password = _configuration.Password;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _configuration.Password = PasswordBox.Password;
            _configuration.Save();
            DialogResult = true;
            Close();
        }
    }
}