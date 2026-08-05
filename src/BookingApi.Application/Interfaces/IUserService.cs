using BookingApi.Application.Dtos;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IUserService
{
    public Task<User> RegisterAsync(
        CreateUserDto dto,
        CancellationToken ct);

    public Task<string> LoginAsync(
        LoginUserDto dto,
        CancellationToken ct);
}
