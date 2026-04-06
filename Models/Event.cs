using System.ComponentModel.DataAnnotations;

namespace EventApi.Models;

public abstract class EventBase : IValidatableObject
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

public class EventRequestDto : EventBase { }

public class Event : EventBase
{
    public required Guid Id { get; set; }
}

public class EventResponseDto : Event { }
