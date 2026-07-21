using BookingApi.Domain.Models;

namespace BookingApi.UnitTests.Helpers;

public static class BookingFactory
{
    public static Booking Generate(
        Guid eventId,
        BookingStatus bookingStatus = BookingStatus.Pending,
        DateTime? createdAt = null,
        Guid? guid = null,
        DateTime? processedAt = null)
    {
        return new Booking
        {
            Id = guid ?? Guid.NewGuid(),
            EventId = eventId,
            Status = bookingStatus,
            CreatedAt = createdAt ?? DateTime.Now,
            ProcessedAt = processedAt
        };
    }
}
