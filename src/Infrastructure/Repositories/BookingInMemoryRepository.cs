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
            return BookingRepositoryResult.EventNotFound(eventId);
        if (!@event.TryReserveSeats())
            return BookingRepositoryResult.NoAvailableSeats();

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
            ? BookingRepositoryResult.Success(booking)
            : BookingRepositoryResult.BookingAlreadyExists(id);
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
                if (!res.IsSuccess) return res;
                return BookingRepositoryResult.EventNotFound(eventId, res.Booking);
            }
        }

        return booking != null
            ? BookingRepositoryResult.Success(booking)
            : BookingRepositoryResult.BookingNotFound(bookingId);
    }

    public Task<IEnumerable<Guid>> TryGetPendingBookingIds(
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var pendingIds = _bookings.Values
            .Where(b => b.Status == BookingStatus.Pending)
            .Select(b => b.Id);

        return Task.FromResult(pendingIds);
    }

    public async Task<BookingRepositoryResult> TryConfirmBooking(
        Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_bookings.TryGetValue(bookingId, out Booking? booking))
            return BookingRepositoryResult.BookingNotFound(bookingId);

        Guid eventId = booking.EventId;
        var @event = await _eventRepository.TryGetByIdAsync(eventId, ct);
        if (@event == null)
        {
            var res = await TryRejectBooking(booking.Id, ct);
            if (!res.IsSuccess) return res;
            return BookingRepositoryResult.EventNotFound(eventId, res.Booking);
        }

        if (booking.Status == BookingStatus.Confirmed)
            return BookingRepositoryResult.Success(booking);

        if (booking.Status != BookingStatus.Pending)
            return BookingRepositoryResult.InvalidStatus(booking, BookingStatus.Pending);

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
            ? BookingRepositoryResult.Success(updatedBooking)
            : BookingRepositoryResult.BookingNotFound(bookingId);
    }

    public async Task<BookingRepositoryResult> TryRejectBooking(Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_bookings.TryGetValue(bookingId, out Booking? booking))
            return BookingRepositoryResult.BookingNotFound(bookingId);

        if (booking.Status == BookingStatus.Rejected)
            return BookingRepositoryResult.Success(booking);

        var updatedBooking = new Booking
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = BookingStatus.Rejected,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = DateTime.Now
        };

        bool isUpdated = _bookings.TryUpdate(bookingId, updatedBooking, booking);
        var @event = await _eventRepository.TryGetByIdAsync(booking.EventId, ct);
        if (isUpdated) @event?.TryReleaseSeats();

        return isUpdated
            ? BookingRepositoryResult.Success(updatedBooking)
            : BookingRepositoryResult.BookingNotFound(bookingId);
    }
}
