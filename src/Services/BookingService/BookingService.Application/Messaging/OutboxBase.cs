namespace BookingService.Application.Messaging;

public abstract class OutboxBase
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string MessageType { get; set; } = null!;
    public Guid CorrelationId { get; set; }
    public string Payload { get; set; } = null!;
    public List<string> Errors { get; set; } = [];
    public uint RowVersion { get; set; }
}
