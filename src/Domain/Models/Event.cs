namespace BookingApi.Domain.Models;

public sealed class Event
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
}
