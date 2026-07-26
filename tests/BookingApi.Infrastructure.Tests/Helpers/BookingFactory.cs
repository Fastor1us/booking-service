using BookingApi.Domain.Models;

namespace BookingApi.IntegrationTests.Helpers;

public static class BookingFactory
{
    public static Booking Generate(
        Guid eventId,
        BookingStatus bookingStatus = BookingStatus.Pending,
        DateTimeOffset? createdAt = null,
        Guid? guid = null,
        DateTimeOffset? processedAt = null)
    {
        return new Booking
        {
            Id = guid ?? Guid.NewGuid(),
            EventId = eventId,
            Status = bookingStatus,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            ProcessedAt = processedAt
        };
    }

    public static List<Booking> Generate(IEnumerable<Guid> eventIds)
    {
        return [.. eventIds.Select(id => Generate(id))];
    }

    public static List<Booking> Generate(Guid eventId, int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Generate(eventId))];
    }
}
