namespace Messaging.Abstractions.Contracts.Commands;

public sealed record ReserveEventSeat(
    Guid BookingId,
    Guid EventId);
