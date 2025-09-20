using CommunityToolkit.Mvvm.ComponentModel;
using CvjmRechnung.ViewModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace CvjmRechnung.Model
{

    public partial class Invoice : ObservableObject
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public string InvoicePath { get; set; }

        [ObservableProperty]
        string orderNumber = "";

        [ObservableProperty]
        string companyName = "";

        [ObservableProperty]
        string firstAndLastName = "";

        [ObservableProperty]
        string streetAndNumber = "";

        [ObservableProperty]
        string postalCodeAndCity = "";

        [ObservableProperty]
        string emailAddress = "";

        [ObservableProperty]
        DateTime? date = DateTime.Now;

        [ObservableProperty]
        ObservableCollection<InvoiceRow> invoiceRows = new();

        public Invoice()
        {
            InvoiceRows.CollectionChanged += InvoiceRows_CollectionChanged;
        }

        private void InvoiceRows_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnInvoiceRowsChanged();
        }

        private void OnInvoiceRowsChanged()
        {
            for (int i = 0; i < InvoiceRows.Count; i++)
            {
                InvoiceRows[i].Position = i + 1;
            }
        }

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
            _logger.Debug($"Invoice saved to {filePath}");
        }

        public Invoice LoadFromXml(string filePath)
        {
            if (!Path.Exists(filePath))
                return new Invoice();
            var serializer = new XmlSerializer(typeof(Invoice));
            using var stream = new FileStream(filePath, FileMode.Open);
            _logger.Debug($"Invoice loaded from {filePath}");
            return serializer.Deserialize(stream) as Invoice;
        }
    }
}
