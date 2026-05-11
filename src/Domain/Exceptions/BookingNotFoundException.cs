namespace BookingApi.Domain.Exceptions;

public class BookingNotFoundException(Guid id)
    : NotFoundException($"Booking with Id '{id}' was not found.")
{
}
