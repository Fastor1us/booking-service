namespace EventService.Application.Dtos;

public abstract class EventRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
}

public class CreateEventDto : EventRequestDto
{
    public int TotalSeats { get; set; }
}

public class UpdateEventDto : EventRequestDto
{
}
