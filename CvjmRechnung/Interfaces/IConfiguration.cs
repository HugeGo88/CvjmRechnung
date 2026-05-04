namespace CvjmRechnung.Interfaces
{
    public interface IConfiguration
    {
        string Password { get; set; }

        string InvoicePath { get; set; }

        void Load();

        void Save();
    }
}
