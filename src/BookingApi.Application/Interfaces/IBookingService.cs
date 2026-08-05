using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IBookingService
{
    public Task<Booking> AddAsync(
        Guid eventId,
        string userLogin,
        CancellationToken ct);

    public Task<Booking> GetByIdAsync(
        Guid bookingId,
        CancellationToken ct);

    public Task CancelAsync(
        Guid bookingId,
        string userLogin,
        CancellationToken ct);
}
