using BookingService.Application.Interfaces;
using BookingService.Domain.Models;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Messaging.Handlers;

public class SeatReservationRejectedHandler(
    IUnitOfWork unitOfWork,
    ILogger<SeatReservationRejectedHandler> logger) : HandlerBase(unitOfWork)
{
    protected override ILogger Logger => logger;

    public override async Task HandleAsync(
        Guid correlationId,
        string payload,
        CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        var isNew = await TryRegisterInboxMessageAsync(
            correlationId, Events.SeatReservationRejected, payload, ct);

        if (!isNew)
        {
            logger.LogDebug(
                "Duplicate message ignored. CorrelationId={CorrelationId}, MessageType={MessageType}",
                correlationId, Events.SeatReservationRejected);
            return;
        }

        var cmd = DeserializePayload<SeatReservationRejected>(payload, correlationId);
        var booking = await _unitOfWork.Bookings
            .FirstOrDefaultAsync(e => e.Id == cmd.BookingId, ct);

        if (booking == null)
        {
            logger.LogWarning(
                "Booking not found for SeatReservationRejected. BookingId={BookingId}, CorrelationId={CorrelationId}",
                cmd.BookingId, correlationId);
            return;
        }

        booking.Status = BookingStatus.Rejected;

        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);

        logger.LogInformation(
            "Booking rejected (no seats / past event). BookingId={BookingId}, EventId={EventId}, CorrelationId={CorrelationId}",
            booking.Id, cmd.EventId, correlationId);
    }
}
