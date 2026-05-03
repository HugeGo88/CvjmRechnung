namespace CvjmRechnung.Interfaces
{
    public interface IConfiguration
    {
        string Password { get; set; }

        void Load();

        void Save();
    }
}
