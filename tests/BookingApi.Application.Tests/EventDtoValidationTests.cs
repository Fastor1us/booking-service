using BookingApi.Application.Dtos;
using BookingApi.Application.Tests.Helpers;
using System.ComponentModel.DataAnnotations;

namespace BookingApi.Application.Tests;

public class EventDtoValidationTests
{
    #region PostEventDto Tests

    [Fact]
    public void PostEventDto_WithValidData_PassesValidation()
    {
        // Arrange
        var dto = EventFactory.Generate<CreateEventDto>(
            "Valid Event", 
            "Valid Description",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(2));

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Empty(validationResults);
    }
    
    [Fact]
    public void PostEventDto_WithEndAtEarlierThanStartAt_ReturnsValidationError()
    {
        // Arrange
        var startAt = DateTime.Now.AddDays(2);
        var endAt = DateTime.Now.AddDays(1);
        var dto = EventFactory.Generate<CreateEventDto>(
            "Invalid Event", 
            "Description",
            startAt,
            endAt);

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Equal("EndAt must be later than StartAt",
            validationResults.First().ErrorMessage);
        Assert.Contains("EndAt",
            validationResults.First().MemberNames);
    }

    [Fact]
    public void PostEventDto_WithEndAtEqualToStartAt_ReturnsValidationError()
    {
        // Arrange
        var sameDateTime = DateTime.Now.AddDays(1);
        var dto = EventFactory.Generate<CreateEventDto>(
            "Invalid Event", 
            "Description",
            sameDateTime,
            sameDateTime);

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Equal("EndAt must be later than StartAt",
            validationResults.First().ErrorMessage);
        Assert.Contains("EndAt",
            validationResults.First().MemberNames);
    }

    [Fact]
    public void PostEventDto_WithNullTitle_ReturnsValidationError()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Title = null!,
            Description = "Description",
            TotalSeats = 20,
            StartAt = DateTime.Now.AddDays(1),
            EndAt = DateTime.Now.AddDays(2)
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Equal("Title is required",
            validationResults.First().ErrorMessage);
        Assert.Contains("Title",
            validationResults.First().MemberNames);
    }

    [Fact]
    public void PostEventDto_WithEmptyTitle_ReturnsValidationError()
    {
        // Arrange
        var dto = EventFactory.Generate<CreateEventDto>(
            "", 
            "Description",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(2));

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Equal("Title is required",
            validationResults.First().ErrorMessage);
        Assert.Contains("Title",
            validationResults.First().MemberNames);
    }

    [Fact]
    public void PostEventDto_WithWhitespaceTitle_ReturnsValidationError()
    {
        // Arrange
        var dto = EventFactory.Generate<CreateEventDto>(
            "   ", 
            "Description",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(2));

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Equal("Title is required",
            validationResults.First().ErrorMessage);
        Assert.Contains("Title",
            validationResults.First().MemberNames);
    }

    [Fact]
    public void PostEventDto_WithDefaultDateTimeStartAt_ReturnsValidationError()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Title = "Valid Title",
            Description = "Description",
            TotalSeats = 20,
            StartAt = default,
            EndAt = DateTime.Now.AddDays(2)
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Equal("StartAt must be a valid date and time",
            validationResults.First().ErrorMessage);
        Assert.Contains("StartAt",
            validationResults.First().MemberNames);
    }

    [Fact]
    public void PostEventDto_WithDefaultDateTimeEndAt_ReturnsValidationError()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Title = "Valid Title",
            Description = "Description",
            TotalSeats = 20,
            StartAt = DateTime.Now.AddDays(1),
            EndAt = default
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Equal("EndAt must be a valid date and time",
            validationResults.First().ErrorMessage);
        Assert.Contains("EndAt",
            validationResults.First().MemberNames);
    }

    [Fact]
    public void PostEventDto_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        var dto = EventFactory.Generate<CreateEventDto>(
            "", 
            "Description",
            DateTime.Now.AddDays(2),
            DateTime.Now.AddDays(1));   // End earlier than Start

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Equal(2, validationResults.Count);
        Assert.Contains(validationResults,
            r => r.ErrorMessage == "Title is required");
        Assert.Contains(validationResults,
            r => r.ErrorMessage == "EndAt must be later than StartAt");
    }

    #endregion

    #region PutEventDto Tests

    [Fact]
    public void PutEventDto_WithValidData_PassesValidation()
    {
        // Arrange
        var dto = new UpdateEventDto
        {
            Title = "Valid Event",
            Description = "Valid Description",
            StartAt = DateTime.Now.AddDays(1),
            EndAt = DateTime.Now.AddDays(2)
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void PutEventDto_WithEndAtEarlierThanStartAt_ReturnsValidationError()
    {
        // Arrange
        var dto = new UpdateEventDto
        {
            Title = "Invalid Event",
            Description = "Description",
            StartAt = DateTime.Now.AddDays(2),
            EndAt = DateTime.Now.AddDays(1)
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Contains(validationResults, r =>
            r.ErrorMessage == "EndAt must be later than StartAt");
    }

    [Fact]
    public void PutEventDto_WithNullTitle_ReturnsValidationError()
    {
        // Arrange
        var dto = new UpdateEventDto
        {
            Title = null!,
            Description = "Description",
            StartAt = DateTime.Now.AddDays(1),
            EndAt = DateTime.Now.AddDays(2)
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Contains(validationResults, r =>
            r.ErrorMessage == "Title is required");
    }

    #endregion

    #region Helper Methods
    private static List<ValidationResult> ValidateDto(object dto)
    {
        var context = new ValidationContext(dto, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        return results;
    }
    #endregion
}
