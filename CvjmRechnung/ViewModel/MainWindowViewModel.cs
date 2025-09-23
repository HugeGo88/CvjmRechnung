using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CvjmRechnung.Interfaces;
using CvjmRechnung.Model;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32;
using QRCoder;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

namespace CvjmRechnung.ViewModel
{
    partial class MainWindowViewModel : ObservableObject
    {
        #region fields

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private string _emptyPdf = $"{Directory.GetCurrentDirectory()}\\Resources\\empty.pdf";

        #endregion

        #region Properties

        public string InvoiceFolder { get => Path.Combine(Path.Combine(Directory.GetParent(Environment.CurrentDirectory).ToString(), "Rechnungen")); }

        [ObservableProperty]
        ObservableCollection<Invoice> invoices = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveFileCommand))]
        [NotifyCanExecuteChangedFor(nameof(GeneratePdfCommand))]
        Invoice selectedItem = new Invoice();

        [ObservableProperty]
        WebView2 webView = new WebView2();

        [ObservableProperty]
        string pdfPath = "";

        public double TotalAmount => SelectedItem.InvoiceRows.Sum(x =>
        {
            return x.TotalPrice;
        });

        private readonly IMailClient iMailClientService;
        private readonly IConfiguration iConfiguration;

        #endregion

        #region constructor
        //public MainWindowViewModel(IMailClient mailClient)
        public MainWindowViewModel(IMailClient mailClientService, IConfiguration configuration)
        {
            if (!Path.Exists(InvoiceFolder))
            {
                Directory.CreateDirectory(InvoiceFolder);
            }
            LoadInvoiceFolder();
            iMailClientService = mailClientService;
            iConfiguration = configuration;
            iConfiguration.Load();
        }

        #endregion

        #region methods
        private void LoadInvoiceFolder()
        {
            var invoiceFiles = Directory.GetFiles(InvoiceFolder, "*.xml");
            var serializer = new XmlSerializer(typeof(Invoice));
            Invoices.Clear();

            foreach (var file in invoiceFiles)
            {
                using var stream = new FileStream(file, FileMode.Open);
                if (serializer.Deserialize(stream) is Invoice invoice)
                {
                    invoice.InvoiceFolder = InvoiceFolder;
                    Invoices.Add(invoice);
                }
            }
        }

        partial void OnSelectedItemChanged(Invoice value)
        {
            PdfPath = _emptyPdf;
            if (value is null) { return; }

            if (value.PdfPath is null) { return; }

            if (!Path.Exists(value.PdfPath)) { return; }

            PdfPath = value.PdfPath;
        }

        private void RenderPdf(string content)
        {
            string templatePathHtml = Path.Combine(Path.GetTempPath(), "template.html");
            templatePathHtml = Path.Combine(Path.GetTempPath(), "template.html");
            if (!Path.Exists(InvoiceFolder))
            {
                Directory.CreateDirectory(InvoiceFolder);
            }
            File.WriteAllText($"{templatePathHtml}", content);

            string pathToExe = getPathForExe("msedge.exe");
            ProcessStartInfo ps = new ProcessStartInfo()
            {
                FileName = pathToExe,
                Arguments = $"--headless --disable-gpu --print-to-pdf-no-header --run-all-compositor-stages-before-draw --virtual-time-budget=5000 --print-to-pdf=\"{SelectedItem.PdfPath}\" \"{templatePathHtml}\"",
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
                PdfPath = SelectedItem.PdfPath;
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
            content = content.Replace("{DATE}", SelectedItem.Date.HasValue ? SelectedItem.Date.Value.ToString("dd MMMM yyyy", CultureInfo.GetCultureInfo("de-DE")) : "");
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
            paymentInfo.InnerText = "Der Gesamtbetrag ist ab Erhalt der Rechnung zahlbar innerhalb von 10 Tagen ohne Abzug.";
            rootDiv.AppendChild(paymentInfo);

            // Add root to document
            xmlDoc.AppendChild(rootDiv);

            // Add QR code image
            string qrCodeContent = $"BCD\r\n001\r\n1\r\nSCT\r\nGENODES1VBB\r\nCVJM Walheim e.V.\r\nDE10604914300390539007\r\nEUR{TotalAmount}\r\nBILL\r\n\r\nRechnung Nr {SelectedItem.OrderNumber} {SelectedItem.FirstAndLastName}\r\nRechnung Nr {SelectedItem.OrderNumber}";

            string qrHtml = CreateQrCodeImgTag(qrCodeContent); // Or any string you want
            var qrDoc = new XmlDocument();
            qrDoc.LoadXml(qrHtml);
            var qrImgNode = xmlDoc.ImportNode(qrDoc.DocumentElement, true);
            rootDiv.AppendChild(qrImgNode);

            // Return HTML string
            return xmlDoc.OuterXml;
        }

        public string CreateQrCodeImgTag(string input)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(input, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var bitmap = qrCode.GetGraphic(20);

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            string base64 = Convert.ToBase64String(ms.ToArray());

            return $"<img src=\"data:image/png;base64,{base64}\" alt=\"QR Code\" width=\"100\" height=\"100\" />";
        }

        #endregion

        #region commands

        [RelayCommand]
        void AddRow()
        {
            _logger.Info("Add new row button pressed");
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
            _logger.Info("Set current date button pressed");
            SelectedItem.Date = DateTime.Now;
        }

        [RelayCommand(CanExecute = nameof(SaveFileAndGenerateMailExcutable))]
        void SaveFile()
        {
            _logger.Info("Save file button pressed");
            SelectedItem.SaveToXml();
        }

        private bool SaveFileAndGenerateMailExcutable()
        {
            return SelectedItem.InvoiceFolder is not null;
        }

        [RelayCommand(CanExecute = nameof(SaveFileAndGenerateMailExcutable))]
        void GeneratePdf()
        {
            _logger.Info("Generate PDF button pressed");
            CalculatePrice();
            PdfPath = _emptyPdf;
            string content = GetHtmlCodeForInvoice();
            RenderPdf(content);
            SaveFile();
        }

        [RelayCommand]
        void AddNewInvoice()
        {
            _logger.Info("Add new invoice button pressed");
            if (Invoices is null) { return; }
            Invoices.Add(new Invoice(InvoiceFolder));
            OnPropertyChanged(nameof(Invoices));
            SelectedItem = Invoices.Last();
        }

        [RelayCommand]
        void SendMail()
        {
            _logger.Info("Send mail button pressed");

            // Show confirmation dialog before sending email
            var result = MessageBox.Show(
                "Soll die Rechnung nun per Email versendet werden?",
                "Email senden",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                _logger.Info("Email send canceled by user.");
                return;
            }

            string body = File.ReadAllText("resources/mail.html");

            try
            {
                iMailClientService.Send(
                    SelectedItem.EmailAddress,
                    $"Rechnung {SelectedItem.OrderNumber}",
                    body,
                    [SelectedItem.PdfPath],
                    iConfiguration.Password
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Email konnte nicht gesendet werden", "Email nicht gesendet", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Email wurde erfolgreich gesendet", "Email gesendet", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        void DeleteSelectedInvoice(KeyEventArgs e)
        {
            if (e.Key == Key.Delete && SelectedItem != null && Invoices.Contains(SelectedItem))
            {
                var result = MessageBox.Show(
                    "Soll die Rechnung wirklich gelöscht werden?",
                    "Löschen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
);

                if (result != MessageBoxResult.Yes)
                {
                    _logger.Info("Delete invoice canceled by user.");
                    return;
                }

                // Delete corresponding files
                if (!string.IsNullOrWhiteSpace(SelectedItem.PdfPath) && File.Exists(SelectedItem.PdfPath))
                {
                    File.Delete(SelectedItem.PdfPath);
                }
                if (!string.IsNullOrWhiteSpace(SelectedItem.XmlPath) && File.Exists(SelectedItem.XmlPath))
                {
                    File.Delete(SelectedItem.XmlPath);
                }

                Invoices.Remove(SelectedItem);
                if (Invoices.Count > 0)
                    SelectedItem = Invoices.Last();
                else
                    SelectedItem = new Invoice(InvoiceFolder);
                e.Handled = true;
            }
        }

        #endregion

    }
}
