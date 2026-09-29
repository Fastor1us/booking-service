using Domain.Models;
using Microsoft.Extensions.Logging;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;
using UserService.Domain.Models;

namespace UserService.Application.Services;

public class UserService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    ILogger<UserService> logger) : IUserService
{
    public async Task<User> RegisterAsync(
        CreateUserDto dto,
        CancellationToken ct)
    {
        var isUserExist = await unitOfWork.UserRepository
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

        unitOfWork.UserRepository.Add(user);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "User {login} with role {role} have been registered",
            user.Login,
            user.Role);

        return user;
    }

    public async Task<string> LoginAsync(
        LoginUserDto dto,
        CancellationToken ct)
    {
        var user = await unitOfWork.UserRepository
            .FirstOrDefaultAsync(e => e.Login == dto.Login, ct)
            ?? throw new UserNotFoundException(dto.Login);

        if (!passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new UserIncorrectPasswordException();
        }

        logger.LogDebug(
            "User {login} with role {role} have been logined",
            user.Login,
            user.Role);

        return tokenGenerator.Generate(user.Id, user.Role);
    }
}
