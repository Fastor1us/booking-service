using BookingApi.Domain.Models;

namespace BookingApi.Application.Dtos;

public abstract class UserRequestDto
{
    public string Login { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class CreateUserDto : UserRequestDto
{
    public UserRole? Role { get; init; } = UserRole.User;
}

public class LoginUserDto : UserRequestDto
{
}
