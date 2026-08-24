using UserService.Domain.Models;

namespace UserService.Application.Interfaces;

public interface ITokenGenerator
{
    string Generate(Guid userId, UserRole role);
}
