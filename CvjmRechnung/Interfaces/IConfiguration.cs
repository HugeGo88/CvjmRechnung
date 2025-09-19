namespace CvjmRechnung.Interfaces
{
    public interface IConfiguration
    {
        public string Password { get; }
        void Load();
        void Save();
    }
}
