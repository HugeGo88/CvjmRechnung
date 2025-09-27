namespace CvjmRechnung.Model
{

    public class EventDetails
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        public string EventName { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string EventId { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public string StartDateString { get => StartDate.ToString("dd.MM.yyyy"); }
        public override string ToString()
        {
            return $"EventId: {EventId} Name: {Name}";
        }

        public static async Task<List<EventDetails>> GetEventDetails()
        {
            IcsReader icsReader = new IcsReader();
            List<EventDetails> eventDetails = new List<EventDetails>();
            List<Ical.Net.CalendarComponents.CalendarEvent> events = await icsReader.ReadIcsFromUrl("http://cvjm-walheim.de/buchungen");
            foreach (var elem in events)
            {
                _logger.Info($"ID: {elem.Uid}  {Environment.NewLine}Description: {elem.Description}");
                EventDetails eventDetail = new EventDetails();
                foreach (var line in elem.Description.Split("\n"))
                {
                    var trimedLine = line.Trim();
                    if (trimedLine.StartsWith("Veranstaltungsname:"))
                    {
                        eventDetail.EventName = trimedLine.Replace("Veranstaltungsname:", "").Trim();
                    }
                    else if (trimedLine.StartsWith("Vor- und Nachname:"))
                    {
                        eventDetail.Name = trimedLine.Replace("Vor- und Nachname:", "").Trim();
                    }
                    else if (trimedLine.StartsWith("Straße und Hausnummer:"))
                    {
                        eventDetail.Street = trimedLine.Replace("Straße und Hausnummer:", "").Trim();
                    }
                    else if (trimedLine.StartsWith("PLZ und Ort:"))
                    {
                        eventDetail.City = trimedLine.Replace("PLZ und Ort:", "").Trim();
                    }
                    else if (trimedLine.StartsWith("Email:"))
                    {
                        eventDetail.Email = trimedLine.Replace("Email:", "").Trim();
                    }
                }
                eventDetail.EventId = elem.Uid.Split("@").First();
                eventDetail.Description = elem.Description;
                eventDetail.StartDate = elem.DtStart.Value.Date;
                eventDetails.Add(eventDetail);
            }

            return eventDetails.OrderBy(x => x.StartDate).Reverse().ToList();
        }

    }
}
