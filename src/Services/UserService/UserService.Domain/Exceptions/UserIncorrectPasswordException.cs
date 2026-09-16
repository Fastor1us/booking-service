using Domain.Exceptions;

namespace UserService.Domain.Exceptions;

public class UserIncorrectPasswordException()
    : NotFoundException($"Password is incorrect.")
{
}
