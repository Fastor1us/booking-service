namespace Messaging.Abstractions.Outbox;

public sealed class OutboxDeadLetter : OutboxBase
{
    public DateTimeOffset MovedToDeadLettersAt { get; set; } = DateTimeOffset.UtcNow;
}
