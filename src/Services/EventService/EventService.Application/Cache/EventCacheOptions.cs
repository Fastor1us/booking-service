namespace EventService.Application.Cache;

public class EventCacheOptions
{
    public TimeSpan EventTtl { get; set; }
    public TimeSpan TopEventsTtl { get; set; }
}
