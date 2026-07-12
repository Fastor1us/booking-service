namespace BookingApi.Domain.Models;

public class Booking
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public Event? Event { get; init; } = null!;
    public required BookingStatus Status { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public uint RowVersion { get; set; }
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Rejected
}
