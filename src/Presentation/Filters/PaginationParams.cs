namespace BookingApi.Presentation.Filters;

using BookingApi.Domain.Exceptions;

public class PaginationParams
{
    public int PageIndex { get; init; }
    public int PageSize { get; init; }

    public PaginationParams(int? pageIndex, int? pageSize)
    {
        PageIndex = pageIndex ?? 1;
        PageSize = pageSize ?? 10;

        List<string> errors = [];

        if (PageIndex < 1)
            errors.Add("PageIndex must be at least 1");

        if (PageSize < 5 || PageSize > 50)
            errors.Add("PageSize must be between 5 and 50");

        if (errors.Count != 0)
            throw new ModelValidationException("Invalid pagination parameters", errors);
    }
}
