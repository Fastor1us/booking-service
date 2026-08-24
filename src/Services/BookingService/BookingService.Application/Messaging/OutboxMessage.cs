namespace BookingService.Application.Messaging;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string MessageType { get; set; } = null!;
    public Guid CorrelationId { get; set; }
    public string Payload { get; set; } = null!;
    public DateTimeOffset? PublishedAtUtc { get; set; }
}
