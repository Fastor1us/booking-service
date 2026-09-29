using EventService.Application.Cache;
using EventService.Application.Interfaces;
using EventService.Domain.Exceptions;
using Messaging.Abstractions.Constants;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Events;
using Messaging.Abstractions.Outbox;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventService.Application.Messaging.Handlers;

public class ReserveSeatHandler(
    IUnitOfWork unitOfWork,
    IEventCache cache,
    ILogger<ReserveSeatHandler> logger) : HandlerBase(unitOfWork)
{
    protected override ILogger Logger => logger;

    public override async Task HandleAsync(
        Guid correlationId,
        string payload,
        CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        var isNew = await TryRegisterInboxMessageAsync(
            correlationId, Commands.ReserveSeat, payload, ct);

        if (!isNew)
        {
            logger.LogDebug(
                "Duplicate message ignored. CorrelationId={CorrelationId}, Command={Command}",
                correlationId, Commands.ReserveSeat);
            return;
        }

        var cmd = DeserializePayload<ReserveEventSeat>(payload, correlationId);
        var @event = await _unitOfWork.Events
            .FirstOrDefaultAsync(e => e.Id == cmd.EventId, ct);

        string? errorMessage = null;
        if (@event == null)
        {
            errorMessage = new EventNotFoundException(cmd.EventId).Message;
        }
        else if (@event.StartAt <= DateTimeOffset.UtcNow)
        {
            errorMessage = new BookingPastEventException(cmd.EventId).Message;
        }
        else if (@event.AvailableSeats < 1)
        {
            errorMessage = new NoAvailableSeatsException(cmd.EventId).Message;
        }

        object message = string.Empty;
        if (errorMessage == null)
        {
            @event!.AvailableSeats--;

            logger.LogInformation(
                "Seat reserved. CorrelationId={CorrelationId}, BookingId={BookingId}, EventId={EventId}, AvailableSeats={AvailableSeats}, ErrorMessage={errorMessage}",
                correlationId, cmd.BookingId, cmd.EventId, @event.AvailableSeats, errorMessage);

            message = new SeatReserved(cmd.BookingId, cmd.EventId);
        }
        else
        {
            logger.LogWarning(
                "Seat reservation rejected. CorrelationId={CorrelationId}, BookingId={BookingId}, EventId={EventId}, Reason={Reason}",
                correlationId, cmd.BookingId, cmd.EventId, errorMessage);

            message = new SeatReservationRejected(cmd.BookingId, cmd.EventId);
        }

        _unitOfWork.OutboxMessages.Add(new OutboxMessage
        {
            Id = correlationId,
            Topic = Topics.EventEventsTopic,
            Key = cmd.EventId.ToString(),
            MessageType = errorMessage == null
                ? Events.SeatReserved
                : Events.SeatReservationRejected,
            CorrelationId = correlationId,
            Payload = JsonSerializer.Serialize(message)
        });

        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);

        if (@event != null)
        {
            await cache.RemoveAsync(EventCacheKey.ForId(@event.Id), ct);
        }
    }
}
