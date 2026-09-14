namespace Messaging.Abstractions.Inbox;

public class InboxMessage
{
    public Guid Id { get; set; }
    public Guid CorrelationId { get; set; }
    public required string MessageType { get; set; }
    public required string Payload { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public uint RowVersion { get; set; }
}

public enum InboxMessageStatus
{
    Pending,
    
}

public enum InboxReason
{
    Unknown = 0,
    MissingHeaders,
    UnknownMessageType,
    DeserializationFailed,
    HandlerFailed,
    Timeout,
    Rejected,
}