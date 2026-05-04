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

        private static string TemplateFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CvjmRechnung",
            "templates");

        public SettingsWindow(IConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            PasswordBox.Password = _configuration.Password;
            InvoicePathTextBox.Text = _configuration.InvoicePath;
            LoadCssFiles();
            LoadTemplateFiles();
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

        private void LoadTemplateFiles()
        {
            Directory.CreateDirectory(TemplateFolder);

            var templateFiles = Directory
                .GetFiles(TemplateFolder, "*.html")
                .Select(Path.GetFileName)
                .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
                .Cast<string>()
                .OrderBy(fileName => fileName)
                .ToList();

            TemplateFileComboBox.ItemsSource = templateFiles;

            if (!string.IsNullOrWhiteSpace(_configuration.SelectedTemplateFile)
                && templateFiles.Contains(_configuration.SelectedTemplateFile))
            {
                TemplateFileComboBox.SelectedItem = _configuration.SelectedTemplateFile;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _configuration.Password = PasswordBox.Password;
            _configuration.InvoicePath = InvoicePathTextBox.Text;
            _configuration.SelectedCssFile = CssFileComboBox.SelectedItem as string ?? string.Empty;
            _configuration.SelectedTemplateFile = TemplateFileComboBox.SelectedItem as string ?? string.Empty;
            _configuration.Save();
            DialogResult = true;
            Close();
        }
    }
}