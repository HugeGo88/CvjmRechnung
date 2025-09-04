using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.WinForms;
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
        WebView2 webView = new WebView2();

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
        string pdfPath = "";

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
            PdfPath = $"{Directory.GetCurrentDirectory()}\\test.pdf";
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
            File.WriteAllBytes($"{Directory.GetCurrentDirectory()}\\Rechnung_{OrderNumber}.pdf", pdf);
            PdfPath = $"{Directory.GetCurrentDirectory()}\\Rechnung_{OrderNumber}.pdf";
        }

        private string GetHtmlCodeForInvoice()
        {
            string content = File.ReadAllText("resources/index.html");
            content = content.Replace("{ADDRESS_FIELD}", File.ReadAllText("resources/addressField.html"));
            content = content.Replace("{ADDRESS_COMPANY}", "");
            content = content.Replace("{CLUB_NAME}", "CVJM Walheim");
            content = content.Replace("{CLUB_STREET}", "Auf der Burg 6");
            content = content.Replace("{CLUB_CITY}", "Walheim");
            content = content.Replace("{CLUB_PHONE}", "+49 (0)175 2000509");
            content = content.Replace("{CLUB_HOMEPAGE}", "cvjm-walheim.de");
            content = content.Replace("{CLUB_EMAIL}", "info@cvjm-walheim.de");
            content = content.Replace("{CLUB_IBAN}", "DE10 6049 1430 0390 5390 07");
            content = content.Replace("{CLUB_BIC}", "GENODES1VBB");
            content = content.Replace("{CLUB_BANK}", "VR-Bank Ludwigsburg eG");
            content = content.Replace("{CLUB_BOARD1}", "Hugo Tausch");
            content = content.Replace("{CLUB_BOARD2}", "Stephanie Alber");
            content = content.Replace("{ORDER_NUMBER}", this.OrderNumber);
            content = content.Replace("{ADDRESS_NAME}", this.FirstAndLastName);
            content = content.Replace("{ADDRESS_STREET}", this.StreetAndNumber);
            content = content.Replace("{ADDRESS_CITY}", this.PostalCodeAndCity);
            content = content.Replace("{DATE}", this.Date.HasValue ? this.Date.Value.ToString("dd MMMM yyyy") : "");
            File.WriteAllText("test.html", content);
            return content;
        }
    }
}
