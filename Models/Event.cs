namespace EventApi.Models;

public abstract class EventBase
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
}

public class EventRequestDto : EventBase { }

public class Event : EventBase
{
    public required Guid Id { get; set; }
}

public class EventResponseDto : Event { }
