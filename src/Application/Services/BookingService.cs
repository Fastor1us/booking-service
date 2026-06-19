using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Services;

public class BookingService(
    IBookingRepository _bookingRepository) : IBookingService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<Booking> CreateBookingAsync(
        Guid eventId, CancellationToken ct)
    {
        BookingRepositoryResult res;

        await _semaphore.WaitAsync(ct);
        try
        {
            res = await _bookingRepository.TryCreateBookingAsync(eventId, ct);
        }
        finally
        {
            _semaphore.Release();
        }

        if (!res.IsSuccess || res.Booking == null)
        {
            if (res.ErrorMessage == null)
                throw new InvalidOperationException(
                    $"Unexpected error in {nameof(CreateBookingAsync)} " +
                    $"for event '{eventId}': repository result has null Details");

            throw res.ErrorType switch
            {
                BookingErrorType.EventNotFound => new EventNotFoundException(eventId),
                BookingErrorType.NoAvailableSeats => new NoAvailableSeatsException(res.ErrorMessage),
                BookingErrorType.BookingAlreadyExists => new InvalidOperationException(res.ErrorMessage),
                BookingErrorType.BookingNotFound => new BookingNotFoundException(res.ErrorMessage),
                BookingErrorType.InvalidStatus => new InvalidOperationException(res.ErrorMessage),
                _ => new InvalidOperationException(res.ErrorMessage)
            };
        }

        return res.Booking;
    }

    public async Task<Booking> GetBookingByIdAsync(
        Guid bookingId, CancellationToken ct)
    {
        var res = await _bookingRepository.TryGetBookingByIdAsync(bookingId, ct);

        if (!res.IsSuccess || res.Booking == null)
        {
            throw new BookingNotFoundException(bookingId);
        }

        return res.Booking;
    }
}
