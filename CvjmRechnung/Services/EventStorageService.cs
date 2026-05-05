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

                // Merge by EventId - only add events not already stored
                var existingIds = new HashSet<string>(existing.Select(e => e.EventId));
                foreach (var evt in newEvents)
                {
                    if (!existingIds.Contains(evt.EventId))
                    {
                        existing.Add(evt);
                        existingIds.Add(evt.EventId);
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
