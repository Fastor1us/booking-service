using System.ComponentModel.DataAnnotations;
using BookingApi.Domain.Constants;

namespace BookingApi.Domain.Models;

public class User
{
    public required Guid Id { get; init; }

    public required string Login
    {
        get;
        init
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ValidationException(
                    UserValidationMessages.LoginRequired);
            }

            if (value.Length < UserConstant.MinLoginLength)
            {
                throw new ValidationException(
                    UserValidationMessages.LoginTooShort);
            }

            if (value.Length >= UserConstant.MaxLoginLength)
            {
                throw new ValidationException(
                    UserValidationMessages.LoginTooLong);
            }

            field = value;
        }
    }

    public required byte[] PasswordHash { get; init; }

    public required UserRole Role { get; init; } = UserRole.User;

    public static void ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ValidationException(
                UserValidationMessages.PasswordRequired);
        }

        if (password.Length < UserConstant.MinPasswordLength)
        {
            throw new ValidationException(
                UserValidationMessages.PasswordTooShort);
        }

        if (password.Length >= UserConstant.MaxPasswordLength)
        {
            throw new ValidationException(
                UserValidationMessages.PasswordTooLong);
        }
    }
}

public enum UserRole
{
    User,
    Admin
}
