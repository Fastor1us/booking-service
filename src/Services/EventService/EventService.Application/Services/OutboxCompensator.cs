using EventService.Application.Interfaces;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Events;
using Messaging.Abstractions.Outbox;
using System.Text.Json;

namespace EventService.Application.Services;

public sealed class OutboxCompensator(IUnitOfWork unitOfWork)
    : IOutboxCompensator
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
                        return false;

                    var @event = await unitOfWork.Events
                        .FirstOrDefaultAsync(e => e.Id == cmd.EventId, ct);

                    if (@event is null)
                        return false;

                    @event.AvailableSeats++;

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
                        return false;

                    var @event = await unitOfWork.Events
                        .FirstOrDefaultAsync(e => e.Id == cmd.EventId, ct);

                    if (@event is null)
                        return false;

                    @event.AvailableSeats--;

                    return true;
                }
            case Events.SeatReservationRejected:
                {
                    return true;
                }

            default:
                return false;
        }
    }
}
