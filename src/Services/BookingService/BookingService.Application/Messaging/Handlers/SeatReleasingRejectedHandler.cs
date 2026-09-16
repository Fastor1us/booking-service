using BookingService.Application.Interfaces;
using BookingService.Domain.Models;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Events;

namespace BookingService.Application.Messaging.Handlers;

public class SeatReleasingRejectedHandler(IUnitOfWork unitOfWork) : HandlerBase(unitOfWork)
{
    public override async Task HandleAsync(
        Guid correlationId,
        string payload,
        CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        var isNew = await TryRegisterInboxMessageAsync(
           correlationId, Events.SeatReleasingRejected, payload, ct);

        if (!isNew)
        {
            return;
        }

        var cmd = DeserializePayload<SeatReleasingRejected>(payload);
        var booking = await _unitOfWork.Bookings
            .FirstOrDefaultAsync(e => e.Id == cmd.BookingId, ct);

        if (booking == null)
        {
            return;
        }

        booking.Status = BookingStatus.Confirmed;

        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);
    }
}
