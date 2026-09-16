namespace EventService.Domain.Exceptions;

public class NoAvailableSeatsException(Guid id)
    : Exception($"Event with Id '{id}' has no available seats left.")
{
}
