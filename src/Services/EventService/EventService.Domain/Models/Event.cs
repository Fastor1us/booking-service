using EventService.Domain.Constants;
using EventService.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace EventService.Domain.Models;

public class Event
{
    public required Guid Id { get; init; }

    public required string Title
    {
        get;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ValidationException(EventValidationMessages.TitleRequired);
            }
            else if (value.Length > EventConstants.TitleMaxLength)
            {
                throw new ValidationException(EventValidationMessages.TitleTooLong);
            }
            else
            {
                field = value;
            }
        }
    }

    public string? Description
    {
        get;
        set
        {
            if (value != null && value.Length > EventConstants.TitleMaxLength)
            {
                throw new ValidationException(EventValidationMessages.DescriptionTooLong);
            }
            else
            {
                field = value;
            }
        }
    }

    public required int TotalSeats
    {
        get;
        set
        {
            if (value < EventConstants.MinTotalSeats)
            {
                throw new ValidationException(EventValidationMessages.TotalSeatsInvalid);
            }
            else
            {
                field = value;
            }
        }
    }

    public int AvailableSeats
    {
        get;
        set
        {
            if (value >= 0 && value <= TotalSeats)
            {
                field = value;
            }
            else if (value > TotalSeats)
            {
                throw new ValidationException(
                    EventValidationMessages.AvailableSeatsCannotBeMoreThanTotalSeats);
            }
            else
            {
                throw new NoAvailableSeatsException(Id);
            }
        }
    }

    public required DateTimeOffset StartAt
    {
        get;
        set
        {
            if (value == default)
            {
                throw new ValidationException(EventValidationMessages.StartAtInvalid);
            }

            field = value;
        }
    }

    public required DateTimeOffset EndAt
    {
        get;
        set
        {
            if (value == default)
            {
                throw new ValidationException(EventValidationMessages.EndAtInvalid);
            }
            if (value <= StartAt)
            {
                throw new ValidationException(EventValidationMessages.EndAtAfterStartAt);
            }

            field = value;
        }
    }

    public uint RowVersion { get; set; }
}
