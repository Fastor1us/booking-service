using Messaging.Abstractions.Outbox;
using Messaging.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Persistence.EfCore;

public sealed class OutboxRepository<TContext>(TContext context)
    : RepositoryBase<OutboxMessage>, IOutboxRepository
    where TContext : DbContext, IOutboxDbContext
{
    public override IQueryable<OutboxMessage> GetQuery(
        QueryTrackerBehavior behavior = QueryTrackerBehavior.Track)
    {
        return behavior switch
        {
            QueryTrackerBehavior.Track =>
                context.OutboxMessages,
            QueryTrackerBehavior.NoTracking =>
                context.OutboxMessages.AsNoTracking(),
            QueryTrackerBehavior.NoTrackingWithIdentityResolution =>
                context.OutboxMessages.AsNoTrackingWithIdentityResolution(),
            _ =>
                context.OutboxMessages,
        };
    }

    public override Task<OutboxMessage?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<OutboxMessage, bool>> predicate,
        CancellationToken ct = default)
    {
        return context.OutboxMessages.FirstOrDefaultAsync(predicate, ct);
    }

    public override void Add(OutboxMessage outboxMessage)
    {
        context.OutboxMessages.Add(outboxMessage);
    }

    public override void Remove(OutboxMessage outboxMessage)
    {
        context.OutboxMessages.Remove(outboxMessage);
    }
}
