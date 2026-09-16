using Messaging.Abstractions.Inbox;
using Messaging.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Persistence.EfCore;

public sealed class InboxRepository<TContext>(TContext context)
    : RepositoryBase<InboxMessage>, IInboxRepository
    where TContext : DbContext, IOutboxDbContext, IInboxDbContext
{
    public override IQueryable<InboxMessage> GetQuery(
        QueryTrackerBehavior behavior = QueryTrackerBehavior.Track)
    {
        return behavior switch
        {
            QueryTrackerBehavior.Track =>
                context.InboxMessages,
            QueryTrackerBehavior.NoTracking =>
                context.InboxMessages.AsNoTracking(),
            QueryTrackerBehavior.NoTrackingWithIdentityResolution =>
                context.InboxMessages.AsNoTrackingWithIdentityResolution(),
            _ =>
                context.InboxMessages,
        };
    }

    public override Task<InboxMessage?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<InboxMessage, bool>> predicate,
        CancellationToken ct = default)
    {
        return context.InboxMessages.FirstOrDefaultAsync(predicate, ct);
    }

    public override void Add(InboxMessage outboxMessage)
    {
        context.InboxMessages.Add(outboxMessage);
    }

    public override void Remove(InboxMessage outboxMessage)
    {
        context.InboxMessages.Remove(outboxMessage);
    }
}
