using EventService.Application.Interfaces;
using EventService.Domain.Exceptions;
using Messaging.Abstractions.Constants;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Events;
using Messaging.Abstractions.Outbox;
using System.Text.Json;

namespace EventService.Application.Messaging.Handlers;

public class ReserveSeatHandler(
    IUnitOfWork unitOfWork,
    IEventCache cache) : HandlerBase(unitOfWork)
{
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
            return;
        }

        var cmd = DeserializePayload<ReserveEventSeat>(payload);
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

            message = new SeatReserved(cmd.BookingId, cmd.EventId);
        }
        else
        {
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
            await cache.RemoveAsync($"event:{@event.Id}", ct);
        }
    }
}
