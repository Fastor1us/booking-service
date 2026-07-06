using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Services;

public class BookingService : IBookingService
{
    public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
