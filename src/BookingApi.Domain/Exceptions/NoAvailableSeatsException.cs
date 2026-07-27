namespace BookingApi.Domain.Exceptions;

public class NoAvailableSeatsException : Exception
{
    public NoAvailableSeatsException(Guid id)
          : base($"Event with Id '{id}' has no available seats left.") { }

    public NoAvailableSeatsException(string message, Exception inner)
        : base(message, inner) { }
}
