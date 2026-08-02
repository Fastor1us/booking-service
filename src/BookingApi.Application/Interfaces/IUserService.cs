using BookingApi.Application.Dtos;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface IUserService
{
    public Task RegisterAsync(
        CreateUserDto dto,
        CancellationToken ct);

    public Task<string> LoginAsync(
        string login,
        string password,
        CancellationToken ct);
}
