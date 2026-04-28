using BookingApi.Domain.Models;

namespace BookingTests.Helpers;

public static class EventFactory
{
    public static Event CreateEvent(
        Guid? guid = null,
        string? title = null,
        DateTime? startAt = null,
        DateTime? endAt = null)
    {
        return new Event
        {
            Id = guid ?? Guid.NewGuid(),
            Title = title ?? "This is event title",
            StartAt = startAt ?? DateTime.Now.AddDays(-1),
            EndAt = endAt ?? DateTime.Now
        };
    }

    public static List<Event> CreateEvents(int count)
    {
        return [.. Enumerable
            .Range(1, count)
            .Select((index) =>
            {
                DateTime date = DateTime.Now;
                return new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Title #" + index.ToString(),
                    StartAt = date.AddDays(-1 * (count - index)),
                    EndAt = date.AddDays(-1 * (count - index - 1))
                };
            })];
    }
}
