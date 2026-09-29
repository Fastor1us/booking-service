using BookingService.Application.Interfaces;
using BookingService.Domain.Models;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Messaging.Handlers;

public class SeatReleasedHandler(
    IUnitOfWork unitOfWork,
    ILogger<SeatReleasedHandler> logger) : HandlerBase(unitOfWork)
{
    protected override ILogger Logger => logger;

    public override async Task HandleAsync(
        Guid correlationId,
        string payload,
        CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        var isNew = await TryRegisterInboxMessageAsync(
            correlationId, Events.SeatReleased, payload, ct);

        if (!isNew)
        {
            logger.LogDebug(
                "Duplicate message ignored. CorrelationId={CorrelationId}, MessageType={MessageType}",
                correlationId, Events.SeatReleased);
            return;
        }

        var cmd = DeserializePayload<SeatReleased>(payload, correlationId);
        var booking = await _unitOfWork.Bookings
            .FirstOrDefaultAsync(e => e.Id == cmd.BookingId, ct);

        if (booking == null)
        {
            logger.LogWarning(
                "Booking not found for SeatReleased. BookingId={BookingId}, CorrelationId={CorrelationId}",
                cmd.BookingId, correlationId);
            return;
        }

        booking.Status = BookingStatus.Cancelled;

        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);

        logger.LogInformation(
            "Booking cancelled (seat released). BookingId={BookingId}, EventId={EventId}, CorrelationId={CorrelationId}",
            booking.Id, cmd.EventId, correlationId);
    }
}
