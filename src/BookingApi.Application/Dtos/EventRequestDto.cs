using System.ComponentModel.DataAnnotations;
using BookingApi.Domain.Constants;

namespace BookingApi.Application.Dtos;

public abstract class EventRequestDto : IValidatableObject
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
            yield return new ValidationResult(
                EventValidationMessages.TitleRequired, [nameof(Title)]);

        if (!string.IsNullOrEmpty(Title)
            && Title.Length > EventConstants.TitleMaxLength)
        {
            yield return new ValidationResult(
                EventValidationMessages.TitleTooLong, [nameof(Title)]);
        }

        if (!string.IsNullOrEmpty(Description)
            && Description.Length > EventConstants.DescriptionMaxLength)
        {
            yield return new ValidationResult(
                EventValidationMessages.DescriptionTooLong, [nameof(Description)]);
        }

        if (StartAt == default)
            yield return new ValidationResult(
                EventValidationMessages.StartAtInvalid, [nameof(StartAt)]);

        if (EndAt == default)
            yield return new ValidationResult(
                EventValidationMessages.EndAtInvalid, [nameof(EndAt)]);

        if (EndAt <= StartAt && StartAt != default && EndAt != default)
            yield return new ValidationResult(
                EventValidationMessages.EndAtAfterStartAt, [nameof(EndAt)]);
    }
}

public class CreateEventDto : EventRequestDto
{
    public int TotalSeats { get; set; }

    public override IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (TotalSeats < EventConstants.MinTotalSeats)
            yield return new ValidationResult(
                EventValidationMessages.TotalSeatsInvalid, [nameof(TotalSeats)]);

        foreach (var result in base.Validate(validationContext))
            yield return result;
    }
}

public class UpdateEventDto : EventRequestDto { }
