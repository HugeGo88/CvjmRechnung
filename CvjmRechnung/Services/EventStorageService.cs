using CvjmRechnung.Model;
using System.IO;
using System.Xml.Serialization;

namespace CvjmRechnung.Services
{
    [Serializable]
    [XmlRoot("Events")]
    public class StoredEventList
    {
        [XmlElement("Event")]
        public List<EventDetails> Events { get; set; } = new();
    }

    public class EventStorageService
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private const string EventsFileName = "events.xml";

        public static void MergeAndSave(List<EventDetails> newEvents, string eventsFolder)
        {
            try
            {
                Directory.CreateDirectory(eventsFolder);
                var filePath = Path.Combine(eventsFolder, EventsFileName);

                var existing = LoadAll(eventsFolder);

                // Build a lookup by EventId so we can upsert: add new events and update changed ones
                var existingById = existing.ToDictionary(e => e.EventId, e => e);
                foreach (var evt in newEvents)
                {
                    if (existingById.TryGetValue(evt.EventId, out var stored))
                    {
                        // Update all fields if anything has changed
                        if (!EventsEqual(stored, evt))
                        {
                            stored.EventName = evt.EventName;
                            stored.Name = evt.Name;
                            stored.Street = evt.Street;
                            stored.City = evt.City;
                            stored.Email = evt.Email;
                            stored.Description = evt.Description;
                            stored.StartDate = evt.StartDate;
                            stored.EndDate = evt.EndDate;
                        }
                    }
                    else
                    {
                        existing.Add(evt);
                        existingById[evt.EventId] = evt;
                    }
                }

                var list = new StoredEventList { Events = existing };
                using var stream = File.Create(filePath);
                var serializer = new XmlSerializer(typeof(StoredEventList));
                serializer.Serialize(stream, list);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving stored events: {ex.Message}", ex.Message);
            }
        }

        private static bool EventsEqual(EventDetails a, EventDetails b)
        {
            return a.EventName == b.EventName
                && a.Name == b.Name
                && a.Street == b.Street
                && a.City == b.City
                && a.Email == b.Email
                && a.Description == b.Description
                && a.StartDate == b.StartDate
                && a.EndDate == b.EndDate;
        }

        public static List<EventDetails> LoadAll(string eventsFolder)
        {
            var filePath = Path.Combine(eventsFolder, EventsFileName);
            if (!File.Exists(filePath))
                return new List<EventDetails>();

            try
            {
                using var stream = File.OpenRead(filePath);
                var serializer = new XmlSerializer(typeof(StoredEventList));
                var list = (StoredEventList?)serializer.Deserialize(stream);
                return list?.Events ?? new List<EventDetails>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading stored events: {ex.Message}", ex.Message);
                return new List<EventDetails>();
            }
        }

        public static List<EventDetails> LoadPastEvents(string eventsFolder)
        {
            var today = DateTime.Today;
            return LoadAll(eventsFolder)
                .Where(e => e.EndDate < today)
                .OrderByDescending(e => e.StartDate)
                .ToList();
        }
    }
}
