using BookingApi.Domain.Models;

namespace BookingApi.Infrastructure.Tests.Helpers;

public static class BookingFactory
{
    public static Booking Generate(
        Guid eventId,
        Guid userId,
        BookingStatus bookingStatus = BookingStatus.Pending,
        DateTimeOffset? createdAt = null,
        Guid? guid = null,
        DateTimeOffset? processedAt = null)
    {
        return new Booking
        {
            Id = guid ?? Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = bookingStatus,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            ProcessedAt = processedAt
        };
    }

    public static List<Booking> Generate(
        IEnumerable<Guid> eventIds,
        Guid userId)
    {
        return [.. eventIds.Select(id => Generate(id, userId))];
    }

    public static List<Booking> Generate(
        Guid eventId,
        Guid userId,
        int count)
    {
        return [.. Enumerable.Range(0, count)
            .Select(_ => Generate(eventId, userId))];
    }
}
