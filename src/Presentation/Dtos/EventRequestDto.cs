using System.ComponentModel.DataAnnotations;

namespace BookingApi.Presentation.Dtos;

public abstract class EventRequestDto : IValidatableObject
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
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

public class CreateEventDto : EventRequestDto
{
    public int TotalSeats { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TotalSeats <= 0)
            yield return new ValidationResult(
                "TotalSeats must be more than 0", [nameof(TotalSeats)]);

        foreach (var result in base.Validate(validationContext))
            yield return result;
    }
}

public class UpdateEventDto : EventRequestDto { }
