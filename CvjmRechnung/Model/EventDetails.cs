namespace CvjmRechnung.Model
{
    public class EventDetails
    {
        public string EventName { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string EventId { get; set; }
        public string Email { get; set; }
        public override string ToString()
        {
            return $"EventId: {EventId} Name: {Name}";
        }

    }
}
