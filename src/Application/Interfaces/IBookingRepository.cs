using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IBookingRepository
{
    public Task<BookingRepositoryResult> TryCreateBookingAsync(
        Guid eventId,
        CancellationToken ct);
    public Task<BookingRepositoryResult> TryGetBookingByIdAsync(
        Guid bookingId,
        CancellationToken ct);
    public Task<IEnumerable<Guid>> TryGetPendingBookingIds(
        CancellationToken ct);
    public Task<BookingRepositoryResult> TryConfirmBooking(
        Guid bookingId,
        CancellationToken ct);

    public Task<BookingRepositoryResult> TryRejectBooking(
        Guid bookingId,
        CancellationToken ct);
}
