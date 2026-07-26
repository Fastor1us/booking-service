namespace BookingApi.Presentation.Domain.Models;

public class Event
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required int TotalSeats { get; init; }
    public int AvailableSeats { get; set; }
    public required DateTimeOffset StartAt { get; set; }
    public required DateTimeOffset EndAt { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; } = [];
    public uint RowVersion { get; set; }
}
