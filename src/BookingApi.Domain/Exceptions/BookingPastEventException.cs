namespace BookingApi.Domain.Exceptions;

public class BookingPastEventException(Guid eventId)
    : Exception($"Event with Id '{eventId}' has already ended.")
{
}
