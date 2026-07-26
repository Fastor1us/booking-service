using BookingApi.Presentation.Domain.Models;

namespace BookingApi.Presentation.Application.Interfaces;

public interface IBookingService
{
    public Task<Booking> AddAsync(
        Guid eventId,
        CancellationToken ct);
    public Task<Booking> GetByIdAsync(
        Guid bookingId,
        CancellationToken ct);
}
