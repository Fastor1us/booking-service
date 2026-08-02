namespace BookingApi.Domain.Exceptions;

public class BookingExceedLimitException(Guid userId)
    : Exception($"User with Id '{userId}' has exceed it active bookings limit.")
{
}
