namespace Messaging.Abstractions.Contracts.Events;

public sealed record SeatReleasingRejected(
    Guid BookingId,
    Guid EventId);
