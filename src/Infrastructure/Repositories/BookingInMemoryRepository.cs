using System.Collections.Concurrent;
using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;

namespace BookingApi.Infrastructure.Repositories;

public class BookingInMemoryRepository : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

    public Task<Booking> CreateBookingAsync(
        Guid eventId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Guid id = Guid.NewGuid();

        while (_bookings.ContainsKey(id))
        {
            id = Guid.NewGuid();
        }

        var booking = new Booking
        {
            Id = id,
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now
        };

        _bookings.TryAdd(id, booking);

        return Task.FromResult(booking);
    }

    public Task<Booking?> GetBookingByIdAsync(
        Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _bookings.TryGetValue(bookingId, out Booking? booking);
        return Task.FromResult(booking);
    }

    public Task<bool> TryGetPendingBooking(
        out Booking? booking, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        booking = _bookings.Values
            .FirstOrDefault(b => b.Status == BookingStatus.Pending);
        return Task.FromResult(booking != null);
    }

    public Task<bool> ConfirmBooking(Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_bookings.TryGetValue(bookingId, out Booking? existingBooking))
        {
            return Task.FromResult(false);
        }

        if (existingBooking.Status == BookingStatus.Confirmed)
        {
            return Task.FromResult(true);
        }

        if (existingBooking.Status != BookingStatus.Pending)
        {
            return Task.FromResult(false);
        }

        var updatedBooking = new Booking
        {
            Id = existingBooking.Id,
            EventId = existingBooking.EventId,
            Status = BookingStatus.Confirmed,
            CreatedAt = existingBooking.CreatedAt,
            ProcessedAt = DateTime.Now
        };

        return Task.FromResult(
            _bookings.TryUpdate(bookingId, updatedBooking, existingBooking));
    }
}
