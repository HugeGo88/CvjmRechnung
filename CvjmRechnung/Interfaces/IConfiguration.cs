namespace CvjmRechnung.Interfaces
{
    public interface IConfiguration
    {
        string Password { get; set; }

        string InvoicePath { get; set; }

        string SettingsRootPath { get; set; }

        string SelectedCssFile { get; set; }

        string SelectedTemplateFile { get; set; }

        string IcsPath { get; set; }

        void Load();

        void Save();
    }
}
