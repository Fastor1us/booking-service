using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IBookingRepository
{
    public Task<Booking> CreateBookingAsync(
        Guid eventId,
        CancellationToken ct);
    public Task<Booking?> GetBookingByIdAsync(
        Guid bookingId,
        CancellationToken ct);
    public Task<bool> TryGetPendingBooking(
        out Booking? booking,
        CancellationToken ct);
    public Task<bool> ConfirmBooking(
        Guid bookingId,
        CancellationToken ct);
}
