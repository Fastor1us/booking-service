using System.ComponentModel.DataAnnotations;
using UserService.Domain.Constants;

namespace UserService.Domain.Models;

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

            if (value.Length < UserConstant.LoginMinLength)
            {
                throw new ValidationException(
                    UserValidationMessages.LoginTooShort);
            }

            if (value.Length >= UserConstant.LoginMaxLength)
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

        if (password.Length < UserConstant.PasswordMinLength)
        {
            throw new ValidationException(
                UserValidationMessages.PasswordTooShort);
        }

        if (password.Length >= UserConstant.PasswordMaxLength)
        {
            throw new ValidationException(
                UserValidationMessages.PasswordTooLong);
        }
    }

    public uint RowVersion { get; set; }
}

public enum UserRole
{
    User,
    Admin
}