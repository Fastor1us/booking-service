namespace BookingService.Application.Messaging;

public sealed class OutboxDeadLetter : OutboxBase
{
    public DateTimeOffset MovedToDeadLettersAt { get; set; } = DateTimeOffset.UtcNow;
}
