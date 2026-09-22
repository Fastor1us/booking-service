namespace EventService.Application.Cache;

public class EventCacheOptions
{
    public TimeSpan EventTtl { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan TopEventsTtl { get; set; } = TimeSpan.FromSeconds(10);
}
