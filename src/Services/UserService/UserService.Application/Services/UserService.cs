using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;
using UserService.Domain.Models;

namespace UserService.Application.Services;

public class UserService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator) : IUserService
{
    public async Task<User> RegisterAsync(
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

        return user;
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
