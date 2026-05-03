using CommunityToolkit.Mvvm.ComponentModel;
using CvjmRechnung.ViewModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CvjmRechnung.Model
{
    public enum InvoiceState
    {
        NEW_INVOICE,
        SAVED,
        PDF_CREATED,
        EMAIL_SEND,
        INVOICE_PAID
    }

    public partial class Invoice : ObservableObject
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Invoice(string invoiceFolder)
        {
            InvoiceFolder = invoiceFolder;
            InvoiceRows.CollectionChanged += InvoiceRows_CollectionChanged;
            InvoiceId = Guid.NewGuid();
        }

        public Invoice()
        {
            InvoiceId = Guid.NewGuid();
            InvoiceRows.CollectionChanged += InvoiceRows_CollectionChanged;
        }

        public string InvoiceFolder { get; set; }

        public string PdfPath { get => @$"{InvoiceFolder}\{InvoiceId}.pdf"; }

        public string XmlPath { get => @$"{InvoiceFolder}\{InvoiceId}.xml"; }

        public Guid InvoiceId { get => _invoiceId; set => _invoiceId = value; }

        [XmlIgnore]
        public double TotalAmount => InvoiceRows.Sum(x => x.TotalPrice);

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
        string description = "";

        [ObservableProperty]
        DateTime? date = DateTime.Now;

        [ObservableProperty]
        InvoiceState state;

        [ObservableProperty]
        ObservableCollection<InvoiceRow> invoiceRows = new();
        private Guid _invoiceId;

        partial void OnInvoiceRowsChanged(ObservableCollection<InvoiceRow> value)
        {
            if (value is null)
            {
                return;
            }

            value.CollectionChanged += InvoiceRows_CollectionChanged;
            foreach (var row in value)
            {
                row.PropertyChanged += InvoiceRow_PropertyChanged;
            }

            OnPropertyChanged(nameof(TotalAmount));
        }

        private void InvoiceRows_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (InvoiceRow row in e.NewItems)
                {
                    row.PropertyChanged += InvoiceRow_PropertyChanged;
                }
            }

            if (e.OldItems is not null)
            {
                foreach (InvoiceRow row in e.OldItems)
                {
                    row.PropertyChanged -= InvoiceRow_PropertyChanged;
                }
            }

            UpdateInvoiceRows();
            OnPropertyChanged(nameof(TotalAmount));
        }

        private void InvoiceRow_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceRow.Quantity) ||
                e.PropertyName == nameof(InvoiceRow.UnitPrice) ||
                e.PropertyName == nameof(InvoiceRow.TotalPrice))
            {
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        private void UpdateInvoiceRows()
        {
            for (int i = 0; i < InvoiceRows.Count; i++)
            {
                InvoiceRows[i].Position = i + 1;
            }
        }

        public void SaveToXml()
        {
            if (State == InvoiceState.NEW_INVOICE)
                State = InvoiceState.SAVED;
            var serializer = new XmlSerializer(typeof(Invoice));
            using var stream = new FileStream(XmlPath, FileMode.Create);
            serializer.Serialize(stream, this);
            _logger.Debug($"Invoice saved to {XmlPath}");
        }

        public Invoice LoadFromXml(string filePath)
        {
            if (!Path.Exists(filePath))
                return new Invoice(InvoiceFolder);
            var serializer = new XmlSerializer(typeof(Invoice));
            using var stream = new FileStream(filePath, FileMode.Open);
            _logger.Debug($"Invoice loaded from {filePath}");
            return serializer.Deserialize(stream) as Invoice;
        }
    }
}
