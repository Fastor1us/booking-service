namespace BookingApi.Domain.Exceptions;

public class UserNotFoundException(string login)
    : NotFoundException($"User with Login '{login}' is not found.")
{
}
