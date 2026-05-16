namespace BookingApi.Domain.Models;

public class Booking
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; set; }
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Rejected
}
