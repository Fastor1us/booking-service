namespace BookingApi.Domain.Exceptions;

public class NoAvailableSeatsException : Exception
{
    public NoAvailableSeatsException(string message)
          : base(message) { }

    public NoAvailableSeatsException(string message, Exception inner)
        : base(message, inner) { }
}
