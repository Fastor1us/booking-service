using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IBookingRepository : IRepository
{
    IQueryable<Booking> GetQuery(QueryTrackerBehavior behavior = default);

    public void Add(Booking booking);
}
