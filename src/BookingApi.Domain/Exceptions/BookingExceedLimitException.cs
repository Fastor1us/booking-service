namespace BookingApi.Domain.Exceptions;

public class BookingExceedLimitException(string userLogin)
    : Exception($"User with Login '{userLogin}' has exceed it active bookings limit.")
{
}
