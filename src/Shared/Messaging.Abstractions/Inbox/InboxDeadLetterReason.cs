namespace Messaging.Abstractions.Inbox;

public enum DeadLetterReason
{
    Unknown = 0,
    MissingHeaders,
    UnknownMessageType,
    DeserializationFailed,
    HandlerFailed,
    Timeout,
    Rejected,
}
