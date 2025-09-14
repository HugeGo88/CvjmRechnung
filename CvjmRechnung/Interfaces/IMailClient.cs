namespace CvjmRechnung.Interfaces
{
    interface IMailClient
    {
        void Send(string to, string subject, string body, List<string> attachments);
    }
}
