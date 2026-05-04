using CvjmRechnung.Interfaces;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows;

namespace CvjmRechnung.View
{
    public partial class SettingsWindow : Window
    {
        private readonly IConfiguration _configuration;

        private static string DefaultSettingsFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CvjmRechnung");

        private string SettingsFolder => string.IsNullOrWhiteSpace(SettingsPathTextBox.Text)
            ? DefaultSettingsFolder
            : SettingsPathTextBox.Text.Trim();

        private string CssFolder => Path.Combine(SettingsFolder, "css");

        private string TemplateFolder => Path.Combine(SettingsFolder, "templates");

        public SettingsWindow(IConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            PasswordBox.Password = _configuration.Password;
            InvoicePathTextBox.Text = _configuration.InvoicePath;
            SettingsPathTextBox.Text = string.IsNullOrWhiteSpace(_configuration.SettingsRootPath)
                ? DefaultSettingsFolder
                : _configuration.SettingsRootPath;
            IcsPathTextBox.Text = _configuration.IcsPath;
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
            CssFileComboBox.ToolTip = $"CSS-Datei aus {CssFolder} auswaehlen";

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
            TemplateFileComboBox.ToolTip = $"HTML-Datei aus {TemplateFolder} auswaehlen";

            if (!string.IsNullOrWhiteSpace(_configuration.SelectedTemplateFile)
                && templateFiles.Contains(_configuration.SelectedTemplateFile))
            {
                TemplateFileComboBox.SelectedItem = _configuration.SelectedTemplateFile;
            }
        }

        private void BrowseSettingsPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Ordner fuer Einstellungen und Templates auswaehlen",
                InitialDirectory = Directory.Exists(SettingsFolder) ? SettingsFolder : DefaultSettingsFolder,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                SettingsPathTextBox.Text = dialog.FolderName;
                LoadCssFiles();
                LoadTemplateFiles();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _configuration.Password = PasswordBox.Password;
            _configuration.InvoicePath = InvoicePathTextBox.Text;
            _configuration.SettingsRootPath = SettingsFolder;
            _configuration.SelectedCssFile = CssFileComboBox.SelectedItem as string ?? string.Empty;
            _configuration.SelectedTemplateFile = TemplateFileComboBox.SelectedItem as string ?? string.Empty;
            _configuration.IcsPath = IcsPathTextBox.Text;

            Directory.CreateDirectory(Path.Combine(SettingsFolder, "css"));
            Directory.CreateDirectory(Path.Combine(SettingsFolder, "templates"));

            _configuration.Save();
            DialogResult = true;
            Close();
        }
    }
}