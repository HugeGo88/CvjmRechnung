using System.IO;
using System.Xml.Serialization;

namespace CvjmRechnung.Services
{
    [Serializable]
    public class ConfigurationData
    {
        public string Password { get; set; } = "";
        public string InvoicePath { get; set; } = "";
        public string SettingsRootPath { get; set; } = "";
        public string SelectedCssFile { get; set; } = "";
        public string SelectedTemplateFile { get; set; } = "";
        public string IcsPath { get; set; } = "";
    }

    public class ConfigurationService : Interfaces.IConfiguration
    {
        private static readonly string DefaultSettingsRootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CvjmRechnung");

        private static readonly string LocationFileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CvjmRechnung",
            "config.location");

        private string _configurationRootPath = DefaultSettingsRootPath;

        private string ConfigFileName => Path.Combine(_configurationRootPath, "config.xml");

        private ConfigurationData _data = new();

        public string Password
        {
            get => _data.Password;
            set => _data.Password = value ?? "";
        }

        public string InvoicePath
        {
            get => _data.InvoicePath;
            set => _data.InvoicePath = value ?? "";
        }

        public string SettingsRootPath
        {
            get => _data.SettingsRootPath;
            set => _data.SettingsRootPath = value ?? "";
        }

        public string SelectedCssFile
        {
            get => _data.SelectedCssFile;
            set => _data.SelectedCssFile = value ?? "";
        }

        public string SelectedTemplateFile
        {
            get => _data.SelectedTemplateFile;
            set => _data.SelectedTemplateFile = value ?? "";
        }

        public string IcsPath
        {
            get => _data.IcsPath;
            set => _data.IcsPath = value ?? "";
        }

        public void Load()
        {
            _configurationRootPath = ResolveConfigurationRootPath();

            if (!File.Exists(ConfigFileName))
            {
                _data = new ConfigurationData
                {
                    SettingsRootPath = _configurationRootPath
                };
                return;
            }

            using var stream = File.OpenRead(ConfigFileName);
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            _data = (ConfigurationData)serializer.Deserialize(stream)!;
            _data.Password ??= "";
            _data.InvoicePath ??= "";
            _data.SettingsRootPath = string.IsNullOrWhiteSpace(_data.SettingsRootPath)
                ? _configurationRootPath
                : _data.SettingsRootPath;
            _data.SelectedCssFile ??= "";
            _data.SelectedTemplateFile ??= "";
            _data.IcsPath ??= "";
        }

        public void Save()
        {
            var settingsRootPath = string.IsNullOrWhiteSpace(_data.SettingsRootPath)
                ? DefaultSettingsRootPath
                : _data.SettingsRootPath.Trim();

            _configurationRootPath = settingsRootPath;
            _data.SettingsRootPath = settingsRootPath;

            Directory.CreateDirectory(_configurationRootPath);
            Directory.CreateDirectory(Path.GetDirectoryName(LocationFileName)!);

            using (var stream = File.Create(ConfigFileName))
            {
                var serializer = new XmlSerializer(typeof(ConfigurationData));
                serializer.Serialize(stream, _data);
            }

            File.WriteAllText(LocationFileName, _configurationRootPath);
        }

        private static string ResolveConfigurationRootPath()
        {
            if (File.Exists(LocationFileName))
            {
                var configuredPath = File.ReadAllText(LocationFileName).Trim();
                if (!string.IsNullOrWhiteSpace(configuredPath))
                {
                    return configuredPath;
                }
            }

            return DefaultSettingsRootPath;
        }
    }
}
