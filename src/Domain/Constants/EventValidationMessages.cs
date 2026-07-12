namespace BookingApi.Domain.Constants;

public static class EventValidationMessages
{
    public const string TitleRequired = "Title is required";
    public const string StartAtInvalid =
        "StartAt must be a valid date and time";
    public const string EndAtInvalid = "EndAt must be a valid date and time";
    public const string EndAtAfterStartAt = "EndAt must be later than StartAt";

    public static readonly string TitleTooLong =
        $"Title cannot exceed {EventConstants.TitleMaxLength} characters";
    public static readonly string DescriptionTooLong =
        $"Description cannot exceed {EventConstants.DescriptionMaxLength} characters";
    public static readonly string TotalSeatsInvalid =
        $"TotalSeats must be at least {EventConstants.MinTotalSeats}";
}
