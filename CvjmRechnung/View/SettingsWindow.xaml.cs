using CvjmRechnung.Interfaces;
using System.IO;
using System.Linq;
using System.Windows;

namespace CvjmRechnung.View
{
    public partial class SettingsWindow : Window
    {
        private readonly IConfiguration _configuration;

        private static string CssFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CvjmRechnung",
            "css");

        public SettingsWindow(IConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            PasswordBox.Password = _configuration.Password;
            InvoicePathTextBox.Text = _configuration.InvoicePath;
            LoadCssFiles();
        }

        private void LoadCssFiles()
        {
            Directory.CreateDirectory(CssFolder);

            var cssFiles = Directory
                .GetFiles(CssFolder, "*.css")
                .Select(Path.GetFileName)
                .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
                .Cast<string>()
                .OrderBy(fileName => fileName)
                .ToList();

            CssFileComboBox.ItemsSource = cssFiles;

            if (!string.IsNullOrWhiteSpace(_configuration.SelectedCssFile)
                && cssFiles.Contains(_configuration.SelectedCssFile))
            {
                CssFileComboBox.SelectedItem = _configuration.SelectedCssFile;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _configuration.Password = PasswordBox.Password;
            _configuration.InvoicePath = InvoicePathTextBox.Text;
            _configuration.SelectedCssFile = CssFileComboBox.SelectedItem as string ?? string.Empty;
            _configuration.Save();
            DialogResult = true;
            Close();
        }
    }
}