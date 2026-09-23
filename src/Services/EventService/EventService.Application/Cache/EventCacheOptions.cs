namespace EventService.Application.Cache;

public class EventCacheOptions
{
    public TimeSpan EventTtl { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan TopEventsTtl { get; set; } = TimeSpan.FromMinutes(10);
}
