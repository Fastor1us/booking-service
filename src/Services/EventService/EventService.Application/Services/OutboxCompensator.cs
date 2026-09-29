using EventService.Application.Interfaces;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Events;
using Messaging.Abstractions.Outbox;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventService.Application.Services;

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
            case Events.SeatReserved:
                {
                    var cmd = JsonSerializer
                        .Deserialize<SeatReleased>(message.Payload);
                    if (cmd is null)
                    {
                        logger.LogError(
                           "Compensation failed: cannot deserialize payload. MessageType={MessageType}, CorrelationId={CorrelationId}",
                           message.MessageType, message.CorrelationId);
                        return false;
                    }

                    var @event = await unitOfWork.Events
                        .FirstOrDefaultAsync(e => e.Id == cmd.EventId, ct);

                    if (@event is null)
                    {
                        logger.LogWarning(
                            "Compensation skipped: event not found. EventId={EventId}, CorrelationId={CorrelationId}",
                            cmd.EventId, message.CorrelationId);
                        return false;
                    }

                    @event.AvailableSeats++;

                    logger.LogInformation(
                        "Compensation applied: seat released. EventId={EventId}, AvailableSeats={AvailableSeats}, CorrelationId={CorrelationId}",
                        @event.Id, @event.AvailableSeats, message.CorrelationId);

                    return true;
                }
            case Events.SeatReleasingRejected:
                {
                    return true;
                }
            case Events.SeatReleased:
                {
                    var cmd = JsonSerializer
                        .Deserialize<SeatReleased>(message.Payload);
                    if (cmd is null)
                    {
                        logger.LogError(
                          "Compensation failed: cannot deserialize payload. MessageType={MessageType}, CorrelationId={CorrelationId}",
                          message.MessageType, message.CorrelationId);
                        return false;
                    }

                    var @event = await unitOfWork.Events
                        .FirstOrDefaultAsync(e => e.Id == cmd.EventId, ct);

                    if (@event is null)
                    {
                        logger.LogWarning(
                            "Compensation skipped: event not found. EventId={EventId}, CorrelationId={CorrelationId}",
                            cmd.EventId, message.CorrelationId);
                        return false;
                    }

                    @event.AvailableSeats--;

                    logger.LogInformation(
                        "Compensation applied: seat reserved. EventId={EventId}, AvailableSeats={AvailableSeats}, CorrelationId={CorrelationId}",
                        @event.Id, @event.AvailableSeats, message.CorrelationId);

                    return true;
                }
            case Events.SeatReservationRejected:
                {
                    return true;
                }

            default:
                {
                    logger.LogWarning(
                        "Compensation not supported for message type. MessageType={MessageType}, CorrelationId={CorrelationId}",
                        message.MessageType, message.CorrelationId);
                    return false;
                }
        }
    }
}
