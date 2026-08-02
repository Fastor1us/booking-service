using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IBookingService
{
    public Task<Booking> AddAsync(
        Guid eventId,
        Guid userId,
        CancellationToken ct);

    public Task<Booking> GetByIdAsync(
        Guid bookingId,
        CancellationToken ct);

    public Task Cancel(
        Guid bookingId,
        Guid userId,
        CancellationToken ct);
}
