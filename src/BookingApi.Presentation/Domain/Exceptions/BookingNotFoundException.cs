namespace BookingApi.Presentation.Domain.Exceptions;

public class BookingNotFoundException : NotFoundException
{
    public BookingNotFoundException(Guid id)
        : base($"Booking with Id '{id}' is not found.")
    {
    }

    public BookingNotFoundException(string message)
        : base(message)
    {
    }
}
