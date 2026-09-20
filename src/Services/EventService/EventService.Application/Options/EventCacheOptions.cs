namespace EventService.Application.Options;

public class EventCacheOptions
{
    public TimeSpan EventTtl { get; set; }
    public TimeSpan TopEventsTtl { get; set; }
}
