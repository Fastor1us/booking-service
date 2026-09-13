using EventService.Domain.Exceptions;
using EventService.Infrastructure.Persistence;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Inbox;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Messaging.Handlers;

// Topics.BookingCommandsTopic
// GroupIds.EventGroup
// Commands.ReserveSeat
public class ReserveSeatHandler(AppDbContext context) : MessagingHandlerBase<ReserveEventSeat>
{
    public override async Task HandleAsync(
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
            MessageType = Commands.ReserveSeat,
            Payload = payload,
            ReceivedAt = DateTime.UtcNow,
        });

        var cmd = GetCommand(payload);
        var @event = await context.Events
            .FirstOrDefaultAsync(e => e.Id == cmd.EventId, ct);

        try
        {
            if (@event == null)
            {
                throw new EventNotFoundException(cmd.EventId);
            }
            else
            {
                if (@event.StartAt <= DateTimeOffset.UtcNow)
                {
                    throw new BookingPastEventException(cmd.EventId);
                }
                else if (@event.AvailableSeats < 1)
                {
                    throw new NoAvailableSeatsException(cmd.EventId);
                }

                // если есть свободные места и событие еще не началось, то вычитаем место
                // делаем запись в OutboxMessages
                @event!.AvailableSeats--;

                // TODO write to Outbound with Success Command
            }
        }
        catch (Exception ex)
        {
            // TODO write to Outbound with Reject Command and Reason
        }

        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
