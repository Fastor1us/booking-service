namespace EventApi.Models;

public class Event
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
