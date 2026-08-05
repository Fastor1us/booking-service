using BookingApi.Domain.Models;
using System.Security.Cryptography;
using System.Text;

namespace BookingApi.Infrastructure.Tests.Helpers;

public static class UserFactory
{
    public static User Generate(
        string login = "login",
        string password = "password",
        UserRole role = UserRole.User)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Login = login,
            PasswordHash = SHA256.HashData(Encoding.UTF8.GetBytes(password)),
            Role = role,
        };
    }
}
