using CvjmRechnung.Interfaces;
using System.Net.Mail;

namespace CvjmRechnung.Services
{
    public class MailClient : IMailClient
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public void Send(string to, string subject, string body, List<string> attachments)
        {
            //HACK: need to be deleted
            to = "hugo.tausch@gmail.com";

            string from = "hausbelegung@cvjm-walheim.de";
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Using the new SMTP client.";
            message.Body = @"Using this new feature, you can send an email message from an application very easily.";
            SmtpClient client = new SmtpClient("smtp.ionos.de");
            client.Port = 465;
            client.Credentials = new System.Net.NetworkCredential("hausbelegung@cvjm-walheim.de", "cynvi8-jeKwiv-xugwez");
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
