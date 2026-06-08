using System.Collections.Concurrent;
using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;

namespace BookingApi.Infrastructure.Repositories;

public class BookingInMemoryRepository(IEventRepository _eventRepository)
    : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

    public async Task<BookingRepositoryResult> TryCreateBookingAsync(
        Guid eventId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var @event = await _eventRepository.TryGetByIdAsync(eventId, ct);
        if (@event == null)
            return new(false, $"Event with id '{eventId}' is not exist");

        Guid id = Guid.NewGuid();

        while (_bookings.ContainsKey(id))
        {
            id = Guid.NewGuid();
        }

        var booking = new Booking
        {
            Id = id,
            EventId = @event.Id,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now
        };

        return _bookings.TryAdd(id, booking)
            ? new(true, booking)
            : new(false, $"Booking with id '{id}' is already exists", booking);
    }

    public async Task<BookingRepositoryResult> TryGetBookingByIdAsync(
        Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_bookings.TryGetValue(bookingId, out Booking? booking))
        {
            Guid eventId = booking.EventId;
            var @event = await _eventRepository.TryGetByIdAsync(eventId, ct);
            if (@event == null)
            {
                var res = await TryRejectBooking(booking.Id, ct);
                if (!res.Success) return res;
                return new(
                    false, $"Event with id '{eventId}' is not exist", res.Booking);
            }
        }

        return booking != null
            ? new(true, booking)
            : new(false, $"Booking with id '{bookingId}' is not exist");
    }

    public Task<BookingRepositoryResult> TryGetPendingBooking(
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var booking = _bookings.Values
            .FirstOrDefault(b => b.Status == BookingStatus.Pending);

        return booking != null
            ? Task.FromResult(new BookingRepositoryResult(true, booking))
            : Task.FromResult(new BookingRepositoryResult(
                false, "There are no pending bookings at this moment"));
    }

    public async Task<BookingRepositoryResult> TryConfirmBooking(
        Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_bookings.TryGetValue(bookingId, out Booking? booking))
            return new(false, $"Booking with id '{bookingId}' is not exist");

        Guid eventId = booking.EventId;
        var @event = await _eventRepository.TryGetByIdAsync(eventId, ct);
        if (@event == null)
        {
            var res = await TryRejectBooking(booking.Id, ct);
            if (!res.Success) return res;
            return new(false, $"Event with id '{eventId}' is not exist", res.Booking);
        }

        if (booking.Status == BookingStatus.Confirmed)
            return new(true, booking);

        if (booking.Status != BookingStatus.Pending)
            return new(
                false,
                $"Booking with id '{bookingId}' is not in {BookingStatus.Pending} status",
                booking);

        var updatedBooking = new Booking
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = BookingStatus.Confirmed,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = DateTime.Now
        };

        bool isUpdated = _bookings.TryUpdate(bookingId, updatedBooking, booking);

        return isUpdated
            ? new(true, updatedBooking)
            : new(false, $"Booking with id '{bookingId}' is not exist");
    }

    public async Task<BookingRepositoryResult> TryRejectBooking(Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_bookings.TryGetValue(bookingId, out Booking? booking))
            return new(false, $"Booking with id '{bookingId}' is not exist");

        if (booking.Status == BookingStatus.Rejected)
            return new(true, booking);

        var updatedBooking = new Booking
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = BookingStatus.Rejected,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = DateTime.Now
        };

        bool isUpdated = _bookings.TryUpdate(bookingId, updatedBooking, booking);

        return isUpdated
            ? new(true, updatedBooking)
            : new(false, $"Booking with id '{bookingId}' is not exist");
    }
}
