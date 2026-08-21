using System.ComponentModel.DataAnnotations;

namespace BookingService.Domain.Exceptions;

public class ModelValidationException : ValidationException
{
    public IEnumerable<string> Details { get; }

    public ModelValidationException(string message)
        : base(message)
    {
        Details = [];
    }

    public ModelValidationException(string message, IEnumerable<string> details)
        : base(message)
    {
        Details = details;
    }

    public ModelValidationException(string message, IEnumerable<string> details, Exception inner)
        : base(message, inner)
    {
        Details = details;
    }
}
