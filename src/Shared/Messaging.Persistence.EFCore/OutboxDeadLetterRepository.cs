using Messaging.Abstractions.Outbox;
using Messaging.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Persistence.EfCore;

public sealed class OutboxDeadLetterRepository<TContext>(TContext context)
    : RepositoryBase<OutboxDeadLetter>, IOutboxDeadLetterRepository
    where TContext : DbContext, IOutboxDbContext
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
