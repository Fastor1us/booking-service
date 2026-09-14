using BookingService.Application.Interfaces;
using BookingService.Infrastructure.Persistence;
using Messaging.Abstractions.Outbox;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Repositories;

public sealed class OutboxDeadLetterRepository(AppDbContext context)
    : RepositoryBase<OutboxDeadLetter>, IOutboxDeadLetterRepository
{
    public override IQueryable<OutboxDeadLetter> GetQuery(
        QueryTrackerBehavior behavior = QueryTrackerBehavior.Track)
    {
        return behavior switch
        {
            QueryTrackerBehavior.Track =>
                context.OutboxDeadLetters,
            QueryTrackerBehavior.NoTracking =>
                context.OutboxDeadLetters.AsNoTracking(),
            QueryTrackerBehavior.NoTrackingWithIdentityResolution =>
                context.OutboxDeadLetters.AsNoTrackingWithIdentityResolution(),
            _ =>
                context.OutboxDeadLetters,
        };
    }

    public override Task<OutboxDeadLetter?> FirstOrDefaultAsync(
        QueryTrackerBehavior behavior,
        System.Linq.Expressions.Expression<Func<OutboxDeadLetter, bool>> predicate,
        CancellationToken ct = default)
    {
        return GetQuery(behavior).FirstOrDefaultAsync(predicate, ct);
    }

    public override Task<OutboxDeadLetter?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<OutboxDeadLetter, bool>> predicate,
        CancellationToken ct = default)
    {
        return context.OutboxDeadLetters.FirstOrDefaultAsync(predicate, ct);
    }

    public override void Add(OutboxDeadLetter outboxMessage)
    {
        context.OutboxDeadLetters.Add(outboxMessage);
    }

    public override void Remove(OutboxDeadLetter outboxMessage)
    {
        context.OutboxDeadLetters.Remove(outboxMessage);
    }
}
