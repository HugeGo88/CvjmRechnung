using System.IO;
using System.Xml.Serialization;

namespace CvjmRechnung.Services
{
    [Serializable]
    public class ConfigurationData
    {
        public string Password { get; set; } = "";
    }

    public class ConfigurationService : Interfaces.IConfiguration
    {
        private readonly string ConfigFileName = Directory.GetCurrentDirectory() + "\\config.xml";
        private ConfigurationData _data = new ConfigurationData();

        public string Password { get => _data.Password; }

        public void Load()
        {
            if (!File.Exists(ConfigFileName))
            {
                _data = new();
                return;
            }


            using var stream = File.OpenRead(ConfigFileName);
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            _data = (ConfigurationData)serializer.Deserialize(stream);
        }

        public void Save()
        {
            using var stream = File.Create(ConfigFileName);
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            serializer.Serialize(stream, _data);
        }
    }
}
