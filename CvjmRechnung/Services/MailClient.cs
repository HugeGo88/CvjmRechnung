using CvjmRechnung.Interfaces;
using System.Net.Mail;

namespace CvjmRechnung.Services
{
    public class MailClient : IMailClient
    {
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
            }
            catch (Exception ex)
            {
            }
        }
    }
}
