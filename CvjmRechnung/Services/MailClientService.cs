using CvjmRechnung.Interfaces;
using System.Net.Mail;

namespace CvjmRechnung.Services
{
    public class MailClientService : IMailClient
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public void Send(string to, string subject, string body, List<string> attachments, string password)
        {
            string from = "hausbelegung@cvjm-walheim.de";
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            if (attachments != null)
            {
                foreach (var filePath in attachments)
                {
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        message.Attachments.Add(new Attachment(filePath));
                    }
                }
            }

            SmtpClient client = new SmtpClient("smtp.ionos.de");
            client.Port = 587;
            client.Credentials = new System.Net.NetworkCredential("hausbelegung@cvjm-walheim.de", password);
            client.EnableSsl = true;
            try
            {
                client.Send(message);
                _logger.Debug($"E-Mail send to {to}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error sending E-Mail to {to}");
                throw;
            }
        }
    }
}
