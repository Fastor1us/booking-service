using BookingService.Application.Interfaces;
using BookingService.Domain.Exceptions;
using BookingService.Domain.Models;
using Domain.Exceptions;
using Domain.Models;
using Messaging.Abstractions.Constants;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Outbox;
using Messaging.Abstractions.Persistence;
using System.Text.Json;

namespace BookingService.Application.Services;

public class BookingService(IUnitOfWork unitOfWork) : IBookingService
{
    public async Task<Booking> AddAsync(
        Guid eventId,
        Guid userId,
        CancellationToken ct)
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        unitOfWork.BookingRepository.Add(booking);

        var message = new ReserveEventSeat(
            BookingId: booking.Id,
            EventId: booking.EventId);

        Guid correlationId = Guid.NewGuid();

        unitOfWork.OutboxRepository.Add(new OutboxMessage
        {
            Id = correlationId,
            Topic = Topics.BookingCommandsTopic,
            Key = booking.EventId.ToString(),
            MessageType = Commands.ReserveSeat,
            CorrelationId = correlationId,
            Payload = JsonSerializer.Serialize(message)
        });

        await unitOfWork.SaveChangesAsync(ct);

        return booking;
    }

    public async Task<Booking> GetByIdAsync(
        Guid bookingId,
        CancellationToken ct)
    {
        return await unitOfWork.BookingRepository
            .FirstOrDefaultAsync(
                QueryTrackerBehavior.NoTracking,
                e => e.Id == bookingId,
                ct)
            ?? throw new BookingNotFoundException(bookingId);
    }

    public async Task CancelAsync(
        Guid bookingId,
        Guid userId,
        UserRole userRole,
        CancellationToken ct)
    {
        var booking = await unitOfWork.BookingRepository
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new BookingNotFoundException(bookingId);

        var isOwner = booking.UserId == userId;
        var isAdmin = userRole == UserRole.Admin;

        if (!isOwner && !isAdmin)
            throw new ForbiddenException();

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new CancelNotConfirmedBookingException(bookingId);
        }

        var message = new ReleaseEventSeat(
            BookingId: booking.Id,
            EventId: booking.EventId);

        booking.Status = BookingStatus.Cancelling;

        Guid correlationId = Guid.NewGuid();

        unitOfWork.OutboxRepository.Add(new OutboxMessage
        {
            Id = correlationId,
            Topic = Topics.BookingCommandsTopic,
            Key = booking.EventId.ToString(),
            MessageType = Commands.ReleaseSeat,
            CorrelationId = correlationId,
            Payload = JsonSerializer.Serialize(message)
        });

        await unitOfWork.SaveChangesAsync(ct);
    }
}
