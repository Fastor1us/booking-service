using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Infrastructure.Repositories;

public sealed class EFCoreEventRepository(AppDbContext context)
    : EFCoreRepositoryBase<Event>, IEventRepository
{
    public override IQueryable<Event> GetQuery(
        QueryTrackerBehavior behavior = QueryTrackerBehavior.Track)
    {
        return behavior switch
        {
            QueryTrackerBehavior.Track =>
                context.Events,
            QueryTrackerBehavior.NoTracking =>
                context.Events.AsNoTracking(),
            QueryTrackerBehavior.NoTrackingWithIdentityResolution =>
                context.Events.AsNoTrackingWithIdentityResolution(),
            _ =>
                context.Events,
        };
    }

    public override Task<Event?> FirstOrDefaultAsync(
        QueryTrackerBehavior behavior,
        System.Linq.Expressions.Expression<Func<Event, bool>> predicate,
        CancellationToken ct = default)
    {
        return GetQuery(behavior).FirstOrDefaultAsync(predicate, ct);
    }

    public override Task<Event?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<Event, bool>> predicate,
        CancellationToken ct = default)
    {
        return context.Events.FirstOrDefaultAsync(predicate, ct);
    }

    public override void Add(Event @event) => context.Events.Add(@event);

    public Task<int> ExecuteUpdateByIdAsync(
        Guid id,
        string title,
        string? description,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        CancellationToken ct = default)
    {
        return context.Events
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(e => e.Title, _ => title)
                    .SetProperty(e => e.Description, _ => description)
                    .SetProperty(e => e.StartAt, _ => startAt)
                    .SetProperty(e => e.EndAt, _ => endAt),
                ct);
    }

    public Task<int> ExecuteDeleteByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return context.Events
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync(ct);
    }
}
