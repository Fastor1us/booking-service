using BookingApi.Domain.Models;

namespace BookingApi.Application.Interfaces;

public interface ITokenGenerator
{
    string Generate(string login, UserRole role);
}
