namespace EventService.Application.Cache;

public class EventCacheKey
{
    public const string Top10 = "events:top10";
    public static string ForId(Guid id) => $"event:{id}";
}
