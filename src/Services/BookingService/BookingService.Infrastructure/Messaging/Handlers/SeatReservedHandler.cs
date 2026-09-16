using BookingService.Domain.Models;
using BookingService.Infrastructure.Persistence;
using Messaging.Abstractions;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Events;
using Messaging.Abstractions.Inbox;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;
using System.Text.Json;

namespace BookingService.Infrastructure.Messaging.Handlers;

public class SeatReservedHandler(AppDbContext context) : IMessageHandler
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
            MessageType = Events.SeatReserved,
            Payload = payload,
            ReceivedAt = DateTime.UtcNow,
        });

        var cmd = GetCommand<SeatReleased>(payload);
        var booking = await context.Bookings
            .FirstOrDefaultAsync(e => e.Id == cmd.BookingId, ct);

        if (booking == null)
        {
            return;
        }

        booking.Status = BookingStatus.Confirmed;

        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    private static T GetCommand<T>(string payload)
    {
        return JsonSerializer.Deserialize<T>(payload)
            ?? throw new SerializationException($"Is not able payload to serialize to {typeof(T)}");
    }
}
