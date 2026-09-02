namespace BookingService.Application.Messaging;

public sealed class OutboxMessage : OutboxBase
{
    public DateTimeOffset? NextAttemptAt { get; set; } = DateTimeOffset.UtcNow;
    public int RetryCount { get; set; } = 0;
}
