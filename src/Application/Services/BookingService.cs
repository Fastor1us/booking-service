using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Services;

public class BookingService(
    IEventRepository _eventRepository,
    IBookingRepository _bookingRepository) : IBookingService
{
    public async Task<Booking> CreateBookingAsync(
        Guid eventId, CancellationToken ct)
    {
        _ = await _eventRepository.GetByIdAsync(eventId, ct)
            ?? throw new EventNotFoundException(eventId);

        return await _bookingRepository.CreateBookingAsync(eventId, ct);
    }

    public async Task<Booking> GetBookingByIdAsync(
        Guid bookingId, CancellationToken ct)
    {
        return await _bookingRepository.GetBookingByIdAsync(bookingId, ct)
            ?? throw new BookingNotFoundException(bookingId);
    }
}
