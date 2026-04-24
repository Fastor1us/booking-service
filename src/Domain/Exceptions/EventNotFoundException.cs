namespace BookingApi.Domain.Exceptions;

public class EventNotFoundException(Guid id)
    : NotFoundException($"Событие с Id '{id}' не найдено.")
{
}
