using BookingApi.Domain.Models;

namespace BookingApi.Application.Dtos;

public class CreateUserDto
{
    public string Login { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public UserRole? Role { get; init; } = UserRole.User;
}
