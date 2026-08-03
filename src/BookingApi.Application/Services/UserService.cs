using BookingApi.Application.Dtos;
using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Services;

public class UserService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator) : IUserService
{
    public async Task RegisterAsync(
        CreateUserDto dto,
        CancellationToken ct)
    {
        var isUserExist = await unitOfWork.UserReopitory
            .FirstOrDefaultAsync(e => e.Login == dto.Login, ct)
            != null;

        if (isUserExist)
        {
            throw new UserAlreadyExistsException(dto.Login);
        }

        User.ValidatePassword(dto.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = dto.Login,
            PasswordHash = passwordHasher.HashPassword(dto.Password),
            Role = dto.Role ?? UserRole.User
        };

        unitOfWork.UserReopitory.Add(user);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<string> LoginAsync(
        LoginUserDto dto,
        CancellationToken ct)
    {
        var user = await unitOfWork.UserReopitory
            .FirstOrDefaultAsync(e => e.Login == dto.Login, ct)
            ?? throw new UserNotFoundException(dto.Login);

        if (!passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new UserIncorrectPasswordException();
        }

        return tokenGenerator.Generate(dto.Login, user.Role);
    }
}
