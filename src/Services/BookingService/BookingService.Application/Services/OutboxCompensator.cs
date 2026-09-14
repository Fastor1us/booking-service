using BookingService.Application.Interfaces;
using BookingService.Domain.Models;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Outbox;
using System.Text.Json;

namespace BookingService.Application.Services;

public sealed class OutboxCompensator(
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IOutboxCompensator
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
                        return false;

                    var booking = await unitOfWork.BookingRepository
                        .FirstOrDefaultAsync(b => b.Id == cmd.BookingId, ct);

                    if (booking is null)
                        return true;

                    booking.Status = BookingStatus.Rejected;
                    booking.ProcessedAt = timeProvider.GetUtcNow();

                    return true;
                }

            default:
                return false;
        }
    }
}
