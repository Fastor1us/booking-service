namespace BookingService.Application.Sagas;

public interface IBookingSagaOrchestrator
{
    Task HandleAsync(
        EventSeatReserved message,
        CancellationToken ct);

    Task HandleAsync(
        EventSeatReservationRejected message,
        CancellationToken ct);

    Task HandleAsync(
        UserValidated message,
        CancellationToken ct);

    Task HandleAsync(
        UserValidationRejected message,
        CancellationToken ct);

    Task HandleAsync(
        EventSeatReleased message,
        CancellationToken ct);
}
