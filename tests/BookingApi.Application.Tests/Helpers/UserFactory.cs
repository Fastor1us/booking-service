using BookingApi.Application.Dtos;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Tests.Helpers;

public static class UserFactory
{
    public static CreateUserDto GenerateCreateDto(
        string login = "login",
        string password = "password",
        UserRole role = UserRole.User)
    {
        return new()
        {
            Login = login,
            Password = password,
            Role = role,
        };
    }

    public static LoginUserDto GenerateLoginDto(
        string login = "login",
        string password = "password")
    {
        return new()
        {
            Login = login,
            Password = password
        };
    }
}
