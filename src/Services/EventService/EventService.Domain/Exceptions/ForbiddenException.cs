namespace EventService.Domain.Exceptions;

public class ForbiddenException(string message) : Exception(message)
{
    public ForbiddenException() : this("Not enough rights to perform an action.")
    {
    }
}
