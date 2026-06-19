namespace BookingApi.Domain.Exceptions;

public class BookingNotFoundException : NotFoundException
{
    public BookingNotFoundException(Guid id)
        : base($"Booking with Id '{id}' was not found.")
    {
    }

    public BookingNotFoundException(string message)
        : base(message)
    {
    }
}
