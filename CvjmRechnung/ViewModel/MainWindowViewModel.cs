using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CvjmRechnung.Model;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CvjmRechnung.ViewModel
{
    partial class MainWindowViewModel : ObservableObject
    {
        string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CvjmRechnungen\\DateLinks.xml");

        public MainWindowViewModel()
        {
            if (!Directory.Exists(InvoiceFolder))
            {
                Directory.CreateDirectory(InvoiceFolder);
            }

            var invoiceFiles = Directory.GetFiles(InvoiceFolder, "*.xml");
            var serializer = new XmlSerializer(typeof(Invoice));
            Invoices.Clear();

            foreach (var file in invoiceFiles)
            {
                using var stream = new FileStream(file, FileMode.Open);
                if (serializer.Deserialize(stream) is Invoice invoice)
                {
                    Invoices.Add(invoice);
                }
            }
        }

        public string InvoiceFolder { get => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CvjmRechnungen\\Rechnungen")); }
        public string InvoiceFile { get => Path.Combine(InvoiceFolder, $"Rechnung_{SelectedItem.OrderNumber}"); }

        [ObservableProperty]
        ObservableCollection<Invoice> invoices = new();

        [ObservableProperty]
        Invoice selectedItem = new Invoice();

        [ObservableProperty]
        WebView2 webView = new WebView2();

        [ObservableProperty]
        string pdfPath = "";

        public double TotalAmount => SelectedItem.InvoiceRows.Sum(x =>
        {
            return x.TotalPrice;
        });

        [RelayCommand]
        void AddRow()
        {
            SelectedItem.InvoiceRows.Add(new InvoiceRow() { Position = SelectedItem.InvoiceRows.Count + 1 });
            OnPropertyChanged(nameof(TotalAmount));
        }

        [RelayCommand]
        void CalculatePrice()
        {
            OnPropertyChanged(nameof(TotalAmount));
        }

        [RelayCommand]
        void SetCurrentDate()
        {
            SelectedItem.Date = DateTime.Now;
        }

        [RelayCommand]
        void SaveFile()
        {
            if (!Path.Exists(InvoiceFolder))
            {
                Directory.CreateDirectory(InvoiceFolder);
            }
            SelectedItem.SaveToXml($"{InvoiceFile}.xml");
        }

        [RelayCommand]
        void GeneratePdf()
        {
            CalculatePrice();
            PdfPath = $"{Directory.GetCurrentDirectory()}\\empty.pdf";
            string content = GetHtmlCodeForInvoice();
            RenderPdf(content);
        }

        private void RenderPdf(string content)
        {
            string templatePathHtml = Directory.GetCurrentDirectory() + "\\template.html";
            if (!Path.Exists(InvoiceFolder))
            {
                Directory.CreateDirectory(InvoiceFolder);
            }
            File.WriteAllText($"{templatePathHtml}", content);

            string pathToExe = getPathForExe("msedge.exe");
            ProcessStartInfo ps = new ProcessStartInfo()
            {
                FileName = pathToExe,
                Arguments = $"--headless --disable-gpu --print-to-pdf-no-header --run-all-compositor-stages-before-draw --virtual-time-budget=5000 --print-to-pdf=\"{InvoiceFile}.pdf\" \"{templatePathHtml}\"",
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
                PdfPath = $"{InvoiceFile}.pdf";
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
            content = content.Replace("{CONTENT}", CreateInvoiceTableHtml(SelectedItem.InvoiceRows.ToList()));
            content = content.Replace("{ADDRESS_COMPANY}", SelectedItem.CompanyName);
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
            content = content.Replace("{ORDER_NUMBER}", SelectedItem.OrderNumber);
            content = content.Replace("{ADDRESS_NAME}", SelectedItem.FirstAndLastName);
            content = content.Replace("{ADDRESS_STREET}", SelectedItem.StreetAndNumber);
            content = content.Replace("{ADDRESS_CITY}", SelectedItem.PostalCodeAndCity);
            content = content.Replace("{DATE}", SelectedItem.Date.HasValue ? SelectedItem.Date.Value.ToString("dd MMMM yyyy") : "");
            File.WriteAllText("test.html", content);
            return content;
        }

        public string CreateInvoiceTableHtml(List<InvoiceRow> rows)
        {
            var xmlDoc = new XmlDocument();

            // Create root <div>
            var rootDiv = xmlDoc.CreateElement("div");

            // Create <h1>
            var h1 = xmlDoc.CreateElement("h1");
            h1.InnerText = $"Rechnung {SelectedItem.OrderNumber}";
            rootDiv.AppendChild(h1);

            // Create <table>
            var table = xmlDoc.CreateElement("table");
            table.SetAttribute("style", "font-family:'Source Sans Pro',sans-serif; font-size:11pt; width:100%; border-collapse:collapse;");

            // Create <thead>
            var thead = xmlDoc.CreateElement("thead");
            var trHead = xmlDoc.CreateElement("tr");
            string[] headers = { "#", "Anzahl", "Einheit", "Position", "Einzelpreis", "Gesamtpreis" };
            for (int i = 0; i < headers.Length; i++)
            {
                var th = xmlDoc.CreateElement("th");
                if (headers[i] == "Position")
                    th.SetAttribute("style", "padding:8px; width:100%; text-align:right;");
                else
                    th.SetAttribute("style", "padding:8px; text-align:right;");
                th.InnerText = headers[i];
                trHead.AppendChild(th);
            }
            thead.AppendChild(trHead);
            table.AppendChild(thead);

            foreach (var row in rows)
            {
                var tbody = xmlDoc.CreateElement("tbody");
                var trBody = xmlDoc.CreateElement("tr");
                string[] rowValues = { row.Position.ToString(), row.Quantity.ToString(), row.Unit, row.Description, $"{row.UnitPrice} EUR", $"{row.TotalPrice} EUR" };
                for (int i = 0; i < rowValues.Length; i++)
                {
                    var td = xmlDoc.CreateElement("td");
                    if (headers[i] == "Position") // Position column
                        td.SetAttribute("style", "padding:8px; width:100%; text-align:right;");
                    else
                        td.SetAttribute("style", "padding:8px; text-align:right;");
                    td.InnerText = rowValues[i];
                    trBody.AppendChild(td);
                }
                tbody.AppendChild(trBody);
                table.AppendChild(tbody);
            }

            // Create <tfoot>
            var tfoot = xmlDoc.CreateElement("tfoot");
            var trFoot = xmlDoc.CreateElement("tr");
            var tdSumLabel = xmlDoc.CreateElement("td");
            tdSumLabel.SetAttribute("colspan", "5");
            tdSumLabel.SetAttribute("style", "padding:8px; text-align:right; font-weight:bold;");
            tdSumLabel.InnerText = "Gesamtbetrag";
            trFoot.AppendChild(tdSumLabel);

            var tdSumValue = xmlDoc.CreateElement("td");
            tdSumValue.SetAttribute("style", "padding:8px; font-weight:bold; text-align:right;");
            tdSumValue.InnerText = $"{TotalAmount} EUR";
            trFoot.AppendChild(tdSumValue);

            tfoot.AppendChild(trFoot);
            table.AppendChild(tfoot);

            // Add table to root
            rootDiv.AppendChild(table);

            // Add payment info <p>
            var paymentInfo = xmlDoc.CreateElement("p");
            paymentInfo.InnerText = "Der Gesamtbetrag ist ab Erhalt der Rechnung zahlbar innerhalb von 7 Tagen ohne Abzug.";
            rootDiv.AppendChild(paymentInfo);

            // Add root to document
            xmlDoc.AppendChild(rootDiv);

            // Return HTML string
            return xmlDoc.OuterXml;
        }
    }
}
