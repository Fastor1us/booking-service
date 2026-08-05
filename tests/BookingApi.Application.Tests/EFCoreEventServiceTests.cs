using BookingApi.Application.Dtos;
using BookingApi.Application.Tests.Helpers;
using BookingApi.Domain.Exceptions;

namespace BookingApi.Application.Tests;

public class EFCoreEventServiceTests : EFCoreServiceTestsBase
{
    #region GetByIdAsync
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectEvent()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);

        // Act
        var result = await _eventService.GetByIdAsync(addedEvent.Id, _ct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedEvent.Id, result.Id);
        Assert.Equal(addedEvent.Title, result.Title);
        Assert.Equal(addedEvent.TotalSeats, result.TotalSeats);
        Assert.Equal(addedEvent.AvailableSeats, result.AvailableSeats);
        Assert.Equal(addedEvent.StartAt, result.StartAt);
        Assert.Equal(addedEvent.EndAt, result.EndAt);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ThrowsEventNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var expectedException = new EventNotFoundException(nonExistentId);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _eventService.GetByIdAsync(nonExistentId, _ct));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }
    #endregion

    #region GetAllAsync
    // Don't test because InMemoryDatabase doesn't have Transaction workaround
    #endregion

    #region AddAsync
    [Fact]
    public async Task AddAsync_ValidEvent_ReturnsAddedEvent()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>();

        // Act
        var result = await _eventService.AddAsync(createEventDto, _ct);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(createEventDto.Title, result.Title);
        Assert.Equal(createEventDto.TotalSeats, result.TotalSeats);
        Assert.Equal(createEventDto.TotalSeats, result.AvailableSeats);
        Assert.Equal(createEventDto.StartAt, result.StartAt);
        Assert.Equal(createEventDto.EndAt, result.EndAt);
    }
    #endregion

    #region UpdateAsync
    // Don't test because InMemoryDatabase doesn't have ExecuteUpdateAsync workaround
    #endregion

    #region RemoveAsync
    // Don't test because InMemoryDatabase doesn't have ExecuteDeleteByIdAsync workaround
    #endregion
}
