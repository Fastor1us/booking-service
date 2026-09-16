using BookingService.Application.Interfaces;
using BookingService.Domain.Models;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Outbox;
using System.Text.Json;

namespace BookingService.Application.Services;

public sealed class OutboxCompensator(IUnitOfWork unitOfWork)
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

                    var booking = await unitOfWork.Bookings
                        .FirstOrDefaultAsync(e => e.Id == cmd.BookingId, ct);

                    if (booking is null)
                        return false;

                    booking.Status = BookingStatus.Rejected;
                    booking.ProcessedAt = DateTimeOffset.UtcNow;

                    return true;
                }
            case Commands.ReleaseSeat:
                {
                    var cmd = JsonSerializer
                        .Deserialize<ReleaseEventSeat>(message.Payload);
                    if (cmd is null)
                        return false;

                    var booking = await unitOfWork.Bookings
                        .FirstOrDefaultAsync(e => e.Id == cmd.BookingId, ct);

                    if (booking is null)
                        return false;

                    booking.Status = BookingStatus.Confirmed;
                    booking.ProcessedAt = DateTimeOffset.UtcNow;

                    return true;
                }

            default:
                return false;
        }
    }
}
