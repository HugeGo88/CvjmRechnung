using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleHtmlToPdf;
using SimpleHtmlToPdf.Interfaces;
using SimpleHtmlToPdf.Settings;
using SimpleHtmlToPdf.Settings.Enums;
using SimpleHtmlToPdf.UnmanagedHandler;
using System.Collections.ObjectModel;
using System.IO;

namespace CvjmRechnung.ViewModel
{
    partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        List<string> rents = new()
        {
            "Vereinsheim",
            "Gelände",
            "Vereinsheim und Gelände"
        };

        [ObservableProperty]
        string orderNumber = "";

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
        bool isMember;

        [ObservableProperty]
        string rent = "Vereinsheim";

        [ObservableProperty]
        ObservableCollection<InvoiceRow> invoiceRows = new();

        [ObservableProperty]
        double totalAmount = 0;

        [RelayCommand]
        void AddRow()
        {
            InvoiceRows.Add(new InvoiceRow() { Position = InvoiceRows.Count + 1 });
            TotalAmount = InvoiceRows.Sum(x => x.TotalPrice);
        }

        [RelayCommand]
        void SetCurrentDate()
        {
            Date = DateTime.Now;
        }

        [RelayCommand]
        void SaveFile()
        {

        }

        [RelayCommand]
        async Task GeneratePdf()
        {
            var Converter = App.Current.Services.GetService<IConverter>();
            string content = GetHtmlCodeForInvoice();
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                 // Color mode of the output file
                 ColorMode = ColorMode.Color,
                 // Orientation of the output file
                 Orientation = Orientation.Portrait,
                 // Paper size of the output file
                 PaperSize = PaperKind.A4,
             },
                Objects = {
                 new ObjectSettings()
                 {
             
                     // HTML content to convert
                     HtmlContent = content,
                     // The default encoding used.
                     WebSettings = { DefaultEncoding = "utf-8" },
                 },
                   }
            };

            var pdf = Converter.Convert(doc);
            File.WriteAllBytes("test.pdf", pdf);

        }

        private string GetHtmlCodeForInvoice()
        {
            string content = File.ReadAllText("resources/index.html");
            content = content.Replace("{ADDRESS_FIELD}", File.ReadAllText("resources/addressField.html"));
            content = content.Replace("{CLUB_NAME}", "CVJM Walheim");
            content = content.Replace("{CLUB_STREET}", "Auf der Burg 6");
            content = content.Replace("{CLUB_CITY}", "Walheim");
            content = content.Replace("{CLUB_PHONE}", "+49 175 2000509");
            content = content.Replace("{CLUB_HOMEPAGE}", "cvjm-walheim.de");
            content = content.Replace("{ADDRESS_NAME}", this.FirstAndLastName);
            content = content.Replace("{ADDRESS_STREET}", this.StreetAndNumber);
            content = content.Replace("{ADDRESS_STREET}", this.StreetAndNumber);
            content = content.Replace("{ADDRESS_CITY}", this.PostalCodeAndCity);
            content = content.Replace("{DATE}", this.Date.HasValue ? this.Date.Value.ToString("Walheim, dd MMMM yyyy") : "");
            File.WriteAllText("test.html", content);
            return content;
        }
    }
}
