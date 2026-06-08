using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Services;

public class BookingService(
    IBookingRepository _bookingRepository) : IBookingService
{
    public async Task<Booking> CreateBookingAsync(
        Guid eventId, CancellationToken ct)
    {
        var res = await _bookingRepository.TryCreateBookingAsync(eventId, ct);
        if (!res.Success || res.Booking == null)
        {
            if (res.Details == null)
                throw new InvalidOperationException(
                    $"Unexpected error in {nameof(CreateBookingAsync)} " +
                    $"for event '{eventId}': repository result has null Details");

            var details = res.Details.ToLower();

            throw details switch
            {
                _ when details.Contains("event") => new EventNotFoundException(eventId),
                _ => new InvalidOperationException(res.Details)
            };
        }

        return res.Booking;
    }

    public async Task<Booking> GetBookingByIdAsync(
        Guid bookingId, CancellationToken ct)
    {
        var res = await _bookingRepository.TryGetBookingByIdAsync(bookingId, ct);

        if (!res.Success || res.Booking == null)
        {
            throw new BookingNotFoundException(bookingId);
        }

        return res.Booking;
    }
}
