using BookingApi.Domain.Exceptions;

namespace BookingApi.Presentation.Filters;

public class EventFilter
{
    public string? Title { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }

    public EventFilter(string? title, DateTime? from, DateTime? to)
    {
        Title = title;
        From = from;
        To = to;

        List<string> errors = [];

        if (From.HasValue && To.HasValue && To.Value <= From.Value)
        {
            errors.Add("'To' date must be later than 'From' date");
        }

        if (errors.Count != 0)
        {
            throw new ModelValidationException("Invalid filter parameters", errors);
        }
    }
}
