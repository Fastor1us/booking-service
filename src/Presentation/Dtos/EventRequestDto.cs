using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BookingApi.Presentation.Dtos;

public abstract class EventRequestDto : IValidatableObject
{
    [Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public DateTime StartAt { get; set; }
    [Required] public DateTime EndAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Здесь только бизнес-логика, не требующая проверки required
        if (EndAt <= StartAt)
        {
            yield return new ValidationResult(
                "EndAt must be later than StartAt",
                [nameof(EndAt)]
            );
        }

        foreach (var property in GetType().GetProperties())
        {
            var value = property.GetValue(this);

            if (property.GetCustomAttribute<RequiredAttribute>() != null)
            {
                if (value == null)
                {
                    yield return new ValidationResult(
                        $"{property.Name} is required",
                        [property.Name]
                    );
                }

                // Проверка для DateTime
                if (value is DateTime dateTime && dateTime == default)
                {
                    yield return new ValidationResult(
                        $"{property.Name} must be a valid date and time",
                        [property.Name]
                    );
                }

                // Проверка для string
                if (value is string str && string.IsNullOrWhiteSpace(str))
                {
                    yield return new ValidationResult(
                        $"{property.Name} is required",
                        [property.Name]
                    );
                }
            }
        }
    }
}

public class PostEventDto : EventRequestDto { }
public class PutEventDto : EventRequestDto { }
