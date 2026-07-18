using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Infrastructure.Repositories;

public class EFCoreBookingRepository(AppDbContext context) : IBookingRepository
{
    public IQueryable<Booking> GetQuery(
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

    public void Add(Booking booking)
    {
        context.Bookings.Add(booking);
    }
}
