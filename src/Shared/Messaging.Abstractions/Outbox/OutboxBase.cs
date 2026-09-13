namespace Messaging.Abstractions.Outbox;

public abstract class OutboxBase
{
    public required Guid Id { get; set; }
    public required string Topic { get; set; } = null!;
    public required string Key { get; set; } = null!;
    public required string MessageType { get; set; } = null!;
    public required Guid CorrelationId { get; set; }
    public required string Payload { get; set; } = null!;
    public List<string> Errors { get; set; } = [];
    public uint RowVersion { get; set; }
}
