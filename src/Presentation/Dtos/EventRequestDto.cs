using System.ComponentModel.DataAnnotations;

namespace BookingApi.Presentation.Dtos;

public abstract class EventRequestDto : IValidatableObject
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
            yield return new ValidationResult(
                "Title is required", [nameof(Title)]);

        if (StartAt == default)
            yield return new ValidationResult(
                "StartAt must be a valid date and time", [nameof(StartAt)]);

        if (EndAt == default)
            yield return new ValidationResult(
                "EndAt must be a valid date and time", [nameof(EndAt)]);

        if (EndAt <= StartAt && StartAt != default && EndAt != default)
            yield return new ValidationResult(
                "EndAt must be later than StartAt", [nameof(EndAt)]);
    }
}

public class PostEventDto : EventRequestDto { }
public class PutEventDto : EventRequestDto { }
