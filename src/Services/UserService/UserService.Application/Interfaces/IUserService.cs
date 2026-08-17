using UserService.Domain.Models;
using UserService.Application.Dtos;

namespace UserService.Application.Interfaces;

public interface IUserService
{
    public Task<User> RegisterAsync(
        CreateUserDto dto,
        CancellationToken ct);

    public Task<string> LoginAsync(
        LoginUserDto dto,
        CancellationToken ct);
}
