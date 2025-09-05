using CommunityToolkit.Mvvm.ComponentModel;
using CvjmRechnung.ViewModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace CvjmRechnung.Model
{
    public partial class Invoice : ObservableObject
    {
        [ObservableProperty]
        List<string> rents = new()
        {
            "Vereinsheim",
            "Gelände",
            "Vereinsheim und Gelände"
        };

        [ObservableProperty]
        string orderNumber = "000";

        [ObservableProperty]
        string companyName = "Max Mustermann GmbH";

        [ObservableProperty]
        string firstAndLastName = "Max Mustermann";

        [ObservableProperty]
        string streetAndNumber = "Musterstraße 123";

        [ObservableProperty]
        string postalCodeAndCity = "12345 Muststadt";

        [ObservableProperty]
        string emailAddress = "max@musterman.de";

        [ObservableProperty]
        DateTime? date = DateTime.Now;

        [ObservableProperty]
        bool isMember;

        [ObservableProperty]
        string rent = "Vereinsheim";

        [ObservableProperty]
        ObservableCollection<InvoiceRow> invoiceRows = new();

        public void SaveToXml(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var serializer = new XmlSerializer(typeof(Invoice));
            using var stream = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(stream, this);
        }

        public Invoice LoadFromXml(string filePath)
        {
            if (!Path.Exists(filePath))
                return new Invoice();
            var serializer = new XmlSerializer(typeof(Invoice));
            using var stream = new FileStream(filePath, FileMode.Open);
            return serializer.Deserialize(stream) as Invoice;
        }
    }
}
