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
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BookingService.Application.Services;

public class BookingService(
    IUnitOfWork unitOfWork,
    ILogger<BookingService> logger) : IBookingService
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

        unitOfWork.Bookings.Add(booking);

        var message = new ReserveEventSeat(
            BookingId: booking.Id,
            EventId: booking.EventId);

        Guid correlationId = Guid.NewGuid();

        unitOfWork.OutboxMessages.Add(new OutboxMessage
        {
            Id = correlationId,
            Topic = Topics.BookingCommandsTopic,
            Key = booking.EventId.ToString(),
            MessageType = Commands.ReserveSeat,
            CorrelationId = correlationId,
            Payload = JsonSerializer.Serialize(message)
        });

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "Booking created. BookingId={BookingId}, EventId={EventId}, UserId={UserId}, CorrelationId={CorrelationId}",
            booking.Id, booking.EventId, booking.UserId, correlationId);

        return booking;
    }

    public async Task<Booking> GetByIdAsync(
        Guid bookingId,
        CancellationToken ct)
    {
        var booking = await unitOfWork.Bookings
            .FirstOrDefaultAsync(
                QueryTrackerBehavior.NoTracking,
                e => e.Id == bookingId,
                ct);

        if (booking is null)
        {
            logger.LogWarning(
                "Booking not found. BookingId={BookingId}", bookingId);

            throw new BookingNotFoundException(bookingId);
        }

        return booking;
    }

    public async Task<Booking> CancelAsync(
        Guid bookingId,
        Guid userId,
        UserRole userRole,
        CancellationToken ct)
    {
        var booking = await unitOfWork.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking is null)
        {
            logger.LogWarning(
                "Booking not found for cancel. BookingId={BookingId}, UserId={UserId}",
                bookingId, userId);

            throw new BookingNotFoundException(bookingId);
        }

        var isOwner = booking.UserId == userId;
        var isAdmin = userRole == UserRole.Admin;

        if (!isOwner && !isAdmin)
        {
            logger.LogWarning(
                "Cancel forbidden: user is not owner or admin. BookingId={BookingId}, UserId={UserId}, OwnerId={OwnerId}, Role={Role}",
                bookingId, userId, booking.UserId, userRole);

            throw new ForbiddenException();
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            logger.LogWarning(
                "Cancel rejected: booking is not confirmed. BookingId={BookingId}, CurrentStatus={Status}",
                bookingId, booking.Status);

            throw new CancelNotConfirmedBookingException(bookingId);
        }

        var message = new ReleaseEventSeat(
            BookingId: booking.Id,
            EventId: booking.EventId);

        booking.Status = BookingStatus.Cancelling;
        booking.ProcessedAt = DateTime.UtcNow;

        Guid correlationId = Guid.NewGuid();

        unitOfWork.OutboxMessages.Add(new OutboxMessage
        {
            Id = correlationId,
            Topic = Topics.BookingCommandsTopic,
            Key = booking.EventId.ToString(),
            MessageType = Commands.ReleaseSeat,
            CorrelationId = correlationId,
            Payload = JsonSerializer.Serialize(message)
        });

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "Booking cancellation started. BookingId={BookingId}, EventId={EventId}, UserId={UserId}, Role={Role}, CorrelationId={CorrelationId}",
            booking.Id, booking.EventId, userId, userRole, correlationId);

        return booking;
    }
}