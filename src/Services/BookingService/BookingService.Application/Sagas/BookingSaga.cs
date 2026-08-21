namespace BookingService.Application.Sagas;

public sealed class BookingSaga
{
    private BookingSaga()
    {
    }

    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EventId { get; private set; }

    public BookingSagaState State { get; private set; }

    public Guid? EventReservationId { get; private set; }
    public string? FailureReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime DeadlineUtc { get; private set; }

    public static BookingSaga Start(
        Guid sagaId,
        Guid bookingId,
        Guid userId,
        Guid eventId,
        DateTime nowUtc,
        TimeSpan timeout)
    {
        return new BookingSaga
        {
            Id = sagaId,
            BookingId = bookingId,
            UserId = userId,
            EventId = eventId,
            State = BookingSagaState.WaitingForEventReservation,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
            DeadlineUtc = nowUtc.Add(timeout)
        };
    }

    public void EventSeatReserved(
        Guid reservationId,
        DateTime nowUtc)
    {
        if (State != BookingSagaState.WaitingForEventReservation)
            return;

        EventReservationId = reservationId;
        State = BookingSagaState.WaitingForUserValidation;
        UpdatedAtUtc = nowUtc;
    }

    public void EventReservationRejected(
        string reason,
        DateTime nowUtc)
    {
        if (State != BookingSagaState.WaitingForEventReservation)
            return;

        FailureReason = reason;
        State = BookingSagaState.Rejected;
        UpdatedAtUtc = nowUtc;
    }

    public void UserValidated(DateTime nowUtc)
    {
        if (State != BookingSagaState.WaitingForUserValidation)
            return;

        State = BookingSagaState.Confirmed;
        UpdatedAtUtc = nowUtc;
    }

    public void StartCompensation(
        string reason,
        DateTime nowUtc)
    {
        if (State != BookingSagaState.WaitingForUserValidation)
            return;

        FailureReason = reason;
        State = BookingSagaState.CompensatingEventReservation;
        UpdatedAtUtc = nowUtc;
    }

    public void EventSeatReleased(DateTime nowUtc)
    {
        if (State != BookingSagaState.CompensatingEventReservation)
            return;

        State = BookingSagaState.Rejected;
        UpdatedAtUtc = nowUtc;
    }
}
