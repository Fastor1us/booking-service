namespace BookingService.Application.Sagas;

public enum BookingSagaState
{
    WaitingForEventReservation,
    WaitingForUserValidation,
    CompensatingEventReservation,
    Confirmed,
    Rejected,
    Failed
}
