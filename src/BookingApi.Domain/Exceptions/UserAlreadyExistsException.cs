namespace BookingApi.Domain.Exceptions;

public class UserAlreadyExistsException(string login)
    : Exception($"User with Login '{login}' already exist.")
{
}
