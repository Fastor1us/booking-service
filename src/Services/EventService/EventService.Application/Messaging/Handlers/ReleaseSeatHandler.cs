using EventService.Application.Interfaces;
using EventService.Domain.Exceptions;
using Messaging.Abstractions.Constants;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Events;
using Messaging.Abstractions.Outbox;
using System.Text.Json;

namespace EventService.Application.Messaging.Handlers;

public class ReleaseSeatHandler(IUnitOfWork unitOfWork) : HandlerBase(unitOfWork)
{
    public override async Task HandleAsync(
        Guid correlationId,
        string payload,
        CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        var isNew = await TryRegisterInboxMessageAsync(
            correlationId, Commands.ReleaseSeat, payload, ct);

        if (!isNew)
        {
            return;
        }

        var cmd = DeserializePayload<ReleaseEventSeat>(payload);
        var @event = await _unitOfWork.Events
            .FirstOrDefaultAsync(e => e.Id == cmd.EventId, ct);

        string? errorMessage = null;
        if (@event == null)
        {
            errorMessage = new EventNotFoundException(cmd.EventId).Message;
        }
        else if (@event.StartAt <= DateTimeOffset.UtcNow)
        {
            errorMessage = new CancelPastEventException(cmd.EventId).Message;
        }

        object message = string.Empty;
        if (errorMessage == null)
        {
            @event!.AvailableSeats++;

            message = new SeatReleased(cmd.BookingId, cmd.EventId);
        }
        else
        {
            message = new SeatReleasingRejected(cmd.BookingId, cmd.EventId);
        }

        _unitOfWork.OutboxMessages.Add(new OutboxMessage
        {
            Id = correlationId,
            Topic = Topics.EventEventsTopic,
            Key = cmd.EventId.ToString(),
            MessageType = errorMessage == null
                ? Events.SeatReleased
                : Events.SeatReleasingRejected,
            CorrelationId = correlationId,
            Payload = JsonSerializer.Serialize(message)
        });

        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);
    }
}
