using BookingApi.Presentation.Application.Interfaces;
using BookingApi.Presentation.Domain.Models;
using BookingApi.Presentation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Presentation.Infrastructure.Repositories;

public sealed class EFCoreBookingRepository(AppDbContext context)
    : EFCoreRepositoryBase<Booking>, IBookingRepository
{
    public override IQueryable<Booking> GetQuery(
        QueryTrackerBehavior behavior = QueryTrackerBehavior.Track)
    {
        return behavior switch
        {
            QueryTrackerBehavior.Track =>
                context.Bookings,
            QueryTrackerBehavior.NoTracking =>
                context.Bookings.AsNoTracking(),
            QueryTrackerBehavior.NoTrackingWithIdentityResolution =>
                context.Bookings.AsNoTrackingWithIdentityResolution(),
            _ =>
                context.Bookings,
        };
    }

    public override Task<Booking?> FirstOrDefaultAsync(
        QueryTrackerBehavior behavior,
        System.Linq.Expressions.Expression<Func<Booking, bool>> predicate,
        CancellationToken ct = default)
    {
        return GetQuery(behavior).FirstOrDefaultAsync(predicate, ct);
    }

    public override Task<Booking?> FirstOrDefaultAsync(
        System.Linq.Expressions.Expression<Func<Booking, bool>> predicate,
        CancellationToken ct = default)
    {
        return context.Bookings.FirstOrDefaultAsync(predicate, ct);
    }

    public override void Add(Booking booking)
    {
        context.Bookings.Add(booking);
    }
}
