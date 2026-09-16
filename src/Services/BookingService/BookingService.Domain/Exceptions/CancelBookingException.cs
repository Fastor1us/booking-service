namespace BookingService.Domain.Exceptions;

public class CancelNotConfirmedBookingException(Guid bookingId)
    : Exception($"An unconfirmed booking '{bookingId}' cannot be cancelled.")
{
}
