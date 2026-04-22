using System.ComponentModel.DataAnnotations;

namespace BookingApi.Domain.Models.Dtos;

public abstract class EventRequestDto : IValidatableObject
{
    [Required]
    public required string Title { get; set; }
    public string? Description { get; set; }
    [Required]
    public required DateTime StartAt { get; set; }
    [Required]
    public required DateTime EndAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndAt <= StartAt)
        {
            yield return new ValidationResult(
                "EndAt must be later than StartAt",
                [nameof(EndAt)]
            );
        }
    }
}

public class PostEventDto : EventRequestDto { }
public class PutEventDto : EventRequestDto { }
