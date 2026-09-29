using BookingService.Application.Interfaces;
using BookingService.Domain.Models;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Outbox;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BookingService.Application.Services;

public sealed class OutboxCompensator(
    IUnitOfWork unitOfWork,
    ILogger<OutboxCompensator> logger) : IOutboxCompensator
{
    public async Task<bool> TryCompensateAsync(
        OutboxMessage message,
        CancellationToken ct = default)
    {
        switch (message.MessageType)
        {
            case Commands.ReserveSeat:
                {
                    var cmd = JsonSerializer
                        .Deserialize<ReserveEventSeat>(message.Payload);
                    if (cmd is null)
                    {
                        logger.LogError(
                           "Compensation failed: cannot deserialize payload. MessageType={MessageType}, CorrelationId={CorrelationId}",
                           message.MessageType, message.CorrelationId);
                        return false;
                    }

                    var booking = await unitOfWork.Bookings
                        .FirstOrDefaultAsync(e => e.Id == cmd.BookingId, ct);

                    if (booking is null)
                    {
                        logger.LogWarning(
                            "Compensation skipped: booking not found. EventId={BookingId}, CorrelationId={CorrelationId}",
                            cmd.BookingId, message.CorrelationId);
                        return false;
                    }

                    booking.Status = BookingStatus.Rejected;
                    booking.ProcessedAt = DateTimeOffset.UtcNow;

                    logger.LogInformation(
                        "Compensation applied: booking rejected. BookingId={BookingId}, EventId={EventId}, CorrelationId={CorrelationId}",
                       booking.Id, cmd.EventId, message.CorrelationId);

                    return true;
                }
            case Commands.ReleaseSeat:
                {
                    var cmd = JsonSerializer
                        .Deserialize<ReleaseEventSeat>(message.Payload);
                    if (cmd is null)
                    {
                        logger.LogError(
                           "Compensation failed: cannot deserialize payload. MessageType={MessageType}, CorrelationId={CorrelationId}",
                           message.MessageType, message.CorrelationId);
                        return false;
                    }

                    var booking = await unitOfWork.Bookings
                        .FirstOrDefaultAsync(e => e.Id == cmd.BookingId, ct);

                    if (booking is null)
                    {
                        logger.LogWarning(
                            "Compensation skipped: booking not found. EventId={BookingId}, CorrelationId={CorrelationId}",
                            cmd.BookingId, message.CorrelationId);
                        return false;
                    }

                    booking.Status = BookingStatus.Confirmed;
                    booking.ProcessedAt = DateTimeOffset.UtcNow;

                    logger.LogInformation(
                        "Compensation applied: booking confirmed. BookingId={BookingId}, EventId={EventId}, CorrelationId={CorrelationId}",
                       booking.Id, cmd.EventId, message.CorrelationId);

                    return true;
                }

            default:
                return false;
        }
    }
}
