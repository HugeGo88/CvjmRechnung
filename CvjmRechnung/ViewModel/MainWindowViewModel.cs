using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CvjmRechnung.Model;
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
        Invoice invoiceData = new Invoice();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalAmount))]
        ObservableCollection<InvoiceRow> invoiceRows = new();

        [ObservableProperty]
        WebView2 webView = new WebView2();

        [ObservableProperty]
        string pdfPath = "";

        public double TotalAmount => InvoiceRows.Sum(x =>
        {
            return x.TotalPrice;
        });

        [RelayCommand]
        void AddRow()
        {
            InvoiceRows.Add(new InvoiceRow() { Position = InvoiceRows.Count + 1 });
            OnPropertyChanged(nameof(TotalAmount));
        }

        [RelayCommand]
        void CaculatePrice()
        {
            OnPropertyChanged(nameof(TotalAmount));
        }

        [RelayCommand]
        void SetCurrentDate()
        {
            InvoiceData.Date = DateTime.Now;
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
                Arguments = $"--headless --disable-gpu --print-to-pdf-no-header --run-all-compositor-stages-before-draw --virtual-time-budget=5000 --print-to-pdf=\"{Directory.GetCurrentDirectory()}\\Rechnung_{InvoiceData.OrderNumber}.pdf\" \"{Directory.GetCurrentDirectory()}\\temp.html\"",
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
                PdfPath = $"{Directory.GetCurrentDirectory()}\\Rechnung_{InvoiceData.OrderNumber}.pdf";
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
            content = content.Replace("{ORDER_NUMBER}", InvoiceData.OrderNumber);
            content = content.Replace("{ADDRESS_NAME}", InvoiceData.FirstAndLastName);
            content = content.Replace("{ADDRESS_STREET}", InvoiceData.StreetAndNumber);
            content = content.Replace("{ADDRESS_CITY}", InvoiceData.PostalCodeAndCity);
            content = content.Replace("{DATE}", InvoiceData.Date.HasValue ? InvoiceData.Date.Value.ToString("dd MMMM yyyy") : "");
            File.WriteAllText("test.html", content);
            return content;
        }
    }
}
