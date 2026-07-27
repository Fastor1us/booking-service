namespace BookingApi.Presentation.Dtos;

public class EventResponseDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public required DateTimeOffset StartAt { get; set; }
    public required DateTimeOffset EndAt { get; set; }
}
