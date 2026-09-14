using BookingService.Application.Interfaces;
using Messaging.Abstractions.Outbox;

namespace BookingService.Application.Services;

public sealed class OutboxStore(IUnitOfWork unitOfWork) : IOutboxStore
{
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize,
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        var query = unitOfWork.OutboxRepository
            .GetQuery(QueryTrackerBehavior.Track)
            .Where(m => m.NextAttemptAt <= now)
            .OrderBy(m => m.NextAttemptAt)
            .Take(batchSize);

        return await unitOfWork.OutboxRepository.ToListAsync(query, ct);
    }

    public async Task MarkAsPublishedAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default)
    {
        var query = unitOfWork.OutboxRepository
            .GetQuery(QueryTrackerBehavior.Track)
            .Where(m => ids.Contains(m.Id));

        var toRemove = await unitOfWork.OutboxRepository.ToListAsync(query, ct);
        foreach (var m in toRemove)
            unitOfWork.OutboxRepository.Remove(m);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => unitOfWork.SaveChangesAsync(ct);

    public Task MoveToDeadLetterAsync(
        OutboxMessage message,
        CancellationToken ct = default)
    {
        unitOfWork.OutboxRepository.Remove(message);
        unitOfWork.OutboxDeadLetterRepository.Add(CreateDeadLetter(message));
        return Task.CompletedTask;
    }

    private static OutboxDeadLetter CreateDeadLetter(OutboxMessage message)
    {
        return new OutboxDeadLetter
        {
            Id = message.Id,
            Topic = message.Topic,
            Key = message.Key,
            MessageType = message.MessageType,
            CorrelationId = message.CorrelationId,
            Payload = message.Payload,
            Errors = message.Errors
        };
    }
}
