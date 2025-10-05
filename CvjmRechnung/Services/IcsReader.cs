using Ical.Net;
using Ical.Net.CalendarComponents;
using System.Net.Http;

public class IcsReader
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public async Task<List<CalendarEvent>> ReadIcsFromUrl(string url)
    {
        var events = new List<CalendarEvent>();

        try
        {
            // 1. Download the ICS content from the URL
            using (var client = new HttpClient())
            {
                // Note: Some calendar servers require a specific User-Agent header
                // client.DefaultRequestHeaders.UserAgent.ParseAdd("C# HttpClient");

                string icsContent = await client.GetStringAsync(url);

                // 2. Parse the ICS content using iCal.NET
                var calendar = Calendar.Load(icsContent);
                {
                    if (calendar == null)
                    {
                        _logger.Error("Failed to parse calendar data.");
                        return events;
                    }
                    // The .Events property contains all VEVENT components.
                    foreach (var calendarEvent in calendar.Events)
                    {
                        events.Add(calendarEvent);
                    }
                }
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error downloading calendar from URL: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error processing ICS data: {e.Message}");
        }

        return events;
    }
}