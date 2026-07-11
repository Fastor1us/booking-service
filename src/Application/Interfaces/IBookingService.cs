using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IBookingService
{
    public Task<Booking> CreateAsync(
        Guid eventId,
        CancellationToken ct);
    public Task<Booking> GetByIdAsync(
        Guid bookingId,
        CancellationToken ct);
}
