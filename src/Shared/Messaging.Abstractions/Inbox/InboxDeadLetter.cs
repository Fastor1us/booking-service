namespace Messaging.Abstractions.Inbox;

public class InboxDeadLetter
{
    public Guid Id { get; init; }
    public Guid? CorrelationId { get; init; }
    public string? MessageType { get; init; }
    public required string Topic { get; init; }
    public required int Partition { get; init; }
    public required long Offset { get; init; }
    public required string ConsumerGroup { get; init; }
    public required string Payload { get; init; }
    public required string HeadersJson { get; init; }  // Dictionary<string,string> → JSON
    public required DeadLetterReason Reason { get; init; }
    public string? ExceptionMessage { get; init; }
    public DateTimeOffset MovedToDeadLettersAt { get; set; } = DateTimeOffset.UtcNow;
}
