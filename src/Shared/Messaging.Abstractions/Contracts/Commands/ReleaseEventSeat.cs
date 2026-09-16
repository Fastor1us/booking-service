namespace Messaging.Abstractions.Contracts.Commands;

public sealed record ReleaseEventSeat(
    Guid BookingId,
    Guid EventId);
