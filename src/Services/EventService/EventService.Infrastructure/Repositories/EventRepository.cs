using EventService.Application.Interfaces;
using EventService.Domain.Models;
using EventService.Infrastructure.Persistence;
using Messaging.Abstractions.Persistence;
using Messaging.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Repositories;

public sealed class EventRepository(AppDbContext context)
    : RepositoryBase<Event>, IEventRepository
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
        System.Linq.Expressions.Expression<Func<Event, bool>> predicate,
        CancellationToken ct = default)
    {
        return context.Events.FirstOrDefaultAsync(predicate, ct);
    }

    public override void Add(Event @event) => context.Events.Add(@event);
    public override void Remove(Event @event) => context.Events.Remove(@event);

    public Task<int> ExecuteUpdateByIdAsync(
        Event @event,
        CancellationToken ct = default)
    {
        return context.Events
            .Where(e => e.Id == @event.Id)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(e => e.Title, _ => @event.Title)
                    .SetProperty(e => e.Description, _ => @event.Description)
                    .SetProperty(e => e.StartAt, _ => @event.StartAt)
                    .SetProperty(e => e.EndAt, _ => @event.EndAt),
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
