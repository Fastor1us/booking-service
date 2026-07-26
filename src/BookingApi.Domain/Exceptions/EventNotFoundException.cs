namespace BookingApi.Domain.Exceptions;

public class EventNotFoundException(Guid id)
    : NotFoundException($"Event with Id '{id}' is not found.")
{
}
