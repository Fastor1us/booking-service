using BookingApi.Domain.Models;

namespace BookingApi.Domain.Tests.Helpers;

public static class EventFactory
{
    public static Event Generate(
        string? title = null,
        string? description = null,
        DateTime? startAt = null,
        DateTime? endAt = null,
        int? totalSeats = null,
        int? availableSeats = null)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = title ?? "Event title",
            Description = description ?? "Event description",
            TotalSeats = totalSeats ?? 20,
            AvailableSeats = availableSeats ?? totalSeats ?? 20,
            StartAt = startAt ?? DateTime.Now.AddDays(-1),
            EndAt = endAt ?? DateTime.Now
        };
    }

    public static List<Event> Generate(int count)
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
                    TotalSeats = 20,
                    AvailableSeats = 20,
                    StartAt = date.AddDays(-1 * (count - index)),
                    EndAt = date.AddDays(-1 * (count - index - 1))
                };
            })];
    }
}
