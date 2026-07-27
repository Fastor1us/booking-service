using BookingApi.Domain.Constants;
using BookingApi.Domain.Models;
using BookingApi.Domain.Tests.Helpers;
using System.ComponentModel.DataAnnotations;

namespace BookingApi.Domain.Tests;

public class EventValidationTests
{
    [Fact]
    public void Event_WithValidData_PassesValidation()
    {
        // Arrange && Act
        var exception = Record.Exception(() =>
            EventFactory.Generate(
            "Valid Event",
            "Valid Description",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(2)));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Event_WithEndAtEarlierThanStartAt_ReturnsValidationError()
    {
        // Arrange
        var startAt = DateTime.Now.AddDays(2);
        var endAt = DateTime.Now.AddDays(1);

        // Act
        var exception = Record.Exception(() =>
            EventFactory.Generate(
                "Invalid Event",
                "Description",
                startAt,
                endAt));

        // Assert
        Assert.IsType<ValidationException>(exception);
        Assert.Equal(EventValidationMessages.EndAtAfterStartAt, exception.Message);
    }

    [Fact]
    public void Event_WithEndAtEqualToStartAt_ReturnsValidationError()
    {
        // Arrange
        var sameDateTime = DateTime.Now.AddDays(1);

        // Act
        var exception = Record.Exception(() =>
            EventFactory.Generate(
                "Invalid Event",
                "Description",
                sameDateTime,
                sameDateTime));

        // Assert
        Assert.IsType<ValidationException>(exception);
        Assert.Equal(EventValidationMessages.EndAtAfterStartAt, exception.Message);
    }

    [Fact]
    public void Event_WithNullTitle_ReturnsValidationError()
    {
        // Arrange && Act
        var exception = Record.Exception(() =>
            new Event
            {
                Id = Guid.NewGuid(),
                Title = null!,
                Description = "Description",
                TotalSeats = 20,
                StartAt = DateTime.Now.AddDays(1),
                EndAt = DateTime.Now.AddDays(2)
            });

        // Assert
        Assert.IsType<ValidationException>(exception);
        Assert.Equal(EventValidationMessages.TitleRequired, exception.Message);
    }

    [Fact]
    public void Event_WithEmptyTitle_ReturnsValidationError()
    {
        // Arrange && Act
        var exception = Record.Exception(() =>
            EventFactory.Generate(
                "",
                "Description",
                DateTime.Now.AddDays(1),
                DateTime.Now.AddDays(2)));

        // Assert
        Assert.IsType<ValidationException>(exception);
        Assert.Equal(EventValidationMessages.TitleRequired, exception.Message);
    }

    [Fact]
    public void Event_WithWhitespaceTitle_ReturnsValidationError()
    {
        // Arrange && Act
        var exception = Record.Exception(() =>
            EventFactory.Generate(
                "   ",
                "Description",
                DateTime.Now.AddDays(1),
                DateTime.Now.AddDays(2)));

        // Assert
        Assert.IsType<ValidationException>(exception);
        Assert.Equal(EventValidationMessages.TitleRequired, exception.Message);
    }

    [Fact]
    public void Event_WithDefaultDateTimeStartAt_ReturnsValidationError()
    {
        // Arrange && Act
        var exception = Record.Exception(() =>
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Description = "Description",
                TotalSeats = 20,
                StartAt = default,
                EndAt = DateTime.Now.AddDays(2)
            });

        // Assert
        Assert.IsType<ValidationException>(exception);
        Assert.Equal(EventValidationMessages.StartAtInvalid, exception.Message);
    }

    [Fact]
    public void Event_WithDefaultDateTimeEndAt_ReturnsValidationError()
    {
        // Arrange && Act
        var exception = Record.Exception(() =>
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Description = "Description",
                TotalSeats = 20,
                StartAt = DateTime.Now.AddDays(1),
                EndAt = default
            });

        // Assert
        Assert.IsType<ValidationException>(exception);
        Assert.Equal(EventValidationMessages.EndAtInvalid, exception.Message);
    }
}
