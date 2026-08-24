namespace Contracts.EventService.Commands;

public sealed record ReserveEventSeat(
    Guid BookingId,
    Guid UserId,
    Guid EventId);
