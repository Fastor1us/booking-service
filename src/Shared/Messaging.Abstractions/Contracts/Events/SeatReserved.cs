namespace Messaging.Abstractions.Contracts.Events;

public sealed record SeatReserved(
    Guid BookingId,
    Guid EventId);
