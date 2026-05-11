using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IBookingService
{
    public Task<Booking> CreateBookingAsync(
        Guid eventId,
        CancellationToken ct);
    public Task<Booking> GetBookingByIdAsync(
        Guid bookingId,
        CancellationToken ct);
}
