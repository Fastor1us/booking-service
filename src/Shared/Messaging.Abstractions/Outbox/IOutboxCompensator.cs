namespace Messaging.Abstractions.Outbox;

public interface IOutboxCompensator
{
    Task<bool> TryCompensateAsync(
        OutboxMessage message,
        CancellationToken ct = default);
}
