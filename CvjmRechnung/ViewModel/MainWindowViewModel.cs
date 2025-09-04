using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32;
using SimpleHtmlToPdf.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        string orderNumber = "000";

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
        void GeneratePdf()
        {
            PdfPath = $"{Directory.GetCurrentDirectory()}\\test.pdf";
            var Converter = App.Current.Services.GetService<IConverter>();
            string content = GetHtmlCodeForInvoice();
            RenderPdf(content);
        }

        private void RenderPdf(string content)
        {
            File.WriteAllText($"{Directory.GetCurrentDirectory()}\\temp.html", content);

            string pathToExe = getPathForExe("msedge.exe");
            ProcessStartInfo ps = new ProcessStartInfo()
            {
                FileName = pathToExe,
                Arguments = $"--headless --disable-gpu --print-to-pdf-no-header --run-all-compositor-stages-before-draw --virtual-time-budget=5000 --print-to-pdf=\"{Directory.GetCurrentDirectory()}\\Rechnung_{OrderNumber}.pdf\" \"{Directory.GetCurrentDirectory()}\\temp.html\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process converter = Process.Start(ps);
            if (!converter.WaitForExit(5000))
            {
                converter.Kill();
            }
            if (converter.ExitCode != 0)
            {
                Console.WriteLine("An error occured!");
            }
            else
            {
                PdfPath = $"{Directory.GetCurrentDirectory()}\\Rechnung_{OrderNumber}.pdf";
            }
        }

        private string getPathForExe(string fileName)
        {
            string keyBase = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";
            RegistryKey localMachine = Registry.LocalMachine;
            RegistryKey fileKey = localMachine.OpenSubKey(String.Format(@"{0}\{1}", keyBase, fileName));
            object result = null;
            if (fileKey != null)
            {
                result = fileKey.GetValue(String.Empty);
                fileKey.Close();
            }
            return (string)result;
        }

        private string GetHtmlCodeForInvoice()
        {
            string content = File.ReadAllText("resources/index.html");
            content = content.Replace("{ADDRESS_FIELD}", File.ReadAllText("resources/addressField.html"));
            content = content.Replace("{CONTENT}", File.ReadAllText("resources/table.html"));
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
