namespace BookingApi.Domain.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
}
