namespace BookingApi.Domain.Models;

public class User
{
    public required Guid Id { get; init; }
    public required string Login { get; init; }
    public required string Password { get; init; }
    public required UserRole Role { get; init; }
}

public enum UserRole
{
    User,
    Admin
}
