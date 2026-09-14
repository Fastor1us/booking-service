using BookingService.Domain.Models;
using Domain.Models;

namespace BookingService.Application.Interfaces;

public interface IBookingService
{
    public Task<Booking> AddAsync(
        Guid eventId,
        Guid userId,
        CancellationToken ct);

    public Task<Booking> GetByIdAsync(
        Guid bookingId,
        CancellationToken ct);

    public Task CancelAsync(
        Guid bookingId,
        Guid userId,
        UserRole userRole,
        CancellationToken ct);
}
