namespace EventService.Domain.Exceptions;

public class CancelPastEventException(Guid eventId)
    : Exception($"Event with Id '{eventId}' has already started.")
{
}
