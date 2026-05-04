using System.IO;
using System.Xml.Serialization;

namespace CvjmRechnung.Services
{
    [Serializable]
    public class ConfigurationData
    {
        public string Password { get; set; } = "";
        public string InvoicePath { get; set; } = "";
        public string SelectedCssFile { get; set; } = "";
        public string SelectedTemplateFile { get; set; } = "";
    }

    public class ConfigurationService : Interfaces.IConfiguration
    {
        private readonly string ConfigFileName = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CvjmRechnung",
            "config.xml"
        );

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

        public void Load()
        {
            if (!File.Exists(ConfigFileName))
            {
                _data = new();
                return;
            }

            using var stream = File.OpenRead(ConfigFileName);
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            _data = (ConfigurationData)serializer.Deserialize(stream)!;
            _data.Password ??= "";
            _data.InvoicePath ??= "";
            _data.SelectedCssFile ??= "";
            _data.SelectedTemplateFile ??= "";
        }

        public void Save()
        {
            var directory = Path.GetDirectoryName(ConfigFileName)!;
            Directory.CreateDirectory(directory);

            using var stream = File.Create(ConfigFileName);
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            serializer.Serialize(stream, _data);
        }
    }
}
