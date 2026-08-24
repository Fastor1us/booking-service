using BookingService.Application.Interfaces;
using BookingService.Application.Messaging;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Repositories;

public sealed class OutboxRepository(AppDbContext context)
    : RepositoryBase<OutboxMessage>, IOutboxRepository
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
        QueryTrackerBehavior behavior,
        System.Linq.Expressions.Expression<Func<OutboxMessage, bool>> predicate,
        CancellationToken ct = default)
    {
        return GetQuery(behavior).FirstOrDefaultAsync(predicate, ct);
    }

    public override Task<OutboxMessage?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<OutboxMessage, bool>> predicate,
        CancellationToken ct = default)
    {
        return context.OutboxMessages.FirstOrDefaultAsync(predicate, ct);
    }

    public override void Add(OutboxMessage booking)
    {
        context.OutboxMessages.Add(booking);
    }
}
