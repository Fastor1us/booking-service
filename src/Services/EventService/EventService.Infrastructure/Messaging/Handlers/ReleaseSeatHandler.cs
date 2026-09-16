using EventService.Domain.Exceptions;
using EventService.Infrastructure.Persistence;
using Messaging.Abstractions;
using Messaging.Abstractions.Constants;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Events;
using Messaging.Abstractions.Inbox;
using Messaging.Abstractions.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;
using System.Text.Json;

namespace EventService.Infrastructure.Messaging.Handlers;

public class ReleaseSeatHandler(AppDbContext context) : IMessageHandler
{
    public async Task HandleAsync(
        Guid correlationId,
        string payload,
        CancellationToken ct)
    {
        using var transaction = await context.Database.BeginTransactionAsync(ct);

        var existedInboxMessage = await context.InboxMessages
            .FirstOrDefaultAsync(e => e.CorrelationId == correlationId, ct);

        if (existedInboxMessage != null) return;

        context.InboxMessages.Add(new InboxMessage()
        {
            Id = correlationId,
            CorrelationId = correlationId,
            MessageType = Commands.ReleaseSeat,
            Payload = payload,
            ReceivedAt = DateTime.UtcNow,
        });

        var cmd = GetCommand<ReleaseEventSeat>(payload);
        var @event = await context.Events
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

            message = new SeatReleased(
               BookingId: cmd.BookingId,
               EventId: cmd.EventId);
        }
        else
        {
            message = new SeatReleasingRejected(
               BookingId: cmd.BookingId,
               EventId: cmd.EventId);
        }

        context.OutboxMessages.Add(new OutboxMessage
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

        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    private static T GetCommand<T>(string payload)
    {
        return JsonSerializer.Deserialize<T>(payload)
            ?? throw new SerializationException($"Is not able payload to serialize to {typeof(T)}");
    }
}
