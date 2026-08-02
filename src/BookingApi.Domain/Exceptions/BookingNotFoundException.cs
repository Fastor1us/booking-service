namespace BookingApi.Domain.Exceptions;

public class BookingNotFoundException(Guid id)
    : NotFoundException($"Booking with Id '{id}' is not found.")
{
}
