namespace BookingApi.Domain.Constants;

public static class UserValidationMessages
{
    public const string LoginRequired = "Login is required";
    public const string PasswordRequired = "Password is required";

    public static readonly string LoginTooShort =
        $"Login cannot be shorter than {UserConstant.LoginMinLength} characters";
    public static readonly string LoginTooLong =
        $"Login cannot exceed {UserConstant.LoginMaxLength} characters";

    public static readonly string PasswordTooShort =
        $"Password cannot be shorter than {UserConstant.PasswordMinLength} characters";
    public static readonly string PasswordTooLong =
        $"Password cannot exceed {UserConstant.PasswordMaxLength} characters";
}
