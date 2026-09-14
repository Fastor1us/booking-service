namespace Messaging.Abstractions.Contracts.Events;

public sealed record EventSeatReservationRejected(
    Guid BookingId,
    Guid EventId);
