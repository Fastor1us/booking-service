namespace Messaging.Abstractions.Contracts.Events;

public sealed record EventSeatReserved(
    Guid BookingId,
    Guid EventId);
