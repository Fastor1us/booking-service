namespace Messaging.Abstractions.Outbox;

public interface IOutboxStore
{
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize, DateTimeOffset now, CancellationToken ct = default);

    Task MarkAsPublishedAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);

    Task MoveToDeadLetterAsync(
        OutboxMessage message, CancellationToken ct = default);
}
