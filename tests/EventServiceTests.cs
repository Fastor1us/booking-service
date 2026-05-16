using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
using BookingTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookingTests;

public class EventServiceTests
{
    private readonly CancellationToken _ct = new CancellationTokenSource().Token;

    [Fact]
    public async Task Add_ValidEvent_ReturnsAddedEvent()
    {
        // Arrange
        Event @event = EventFactory.CreateEvent();

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Event>(), _ct))
            .ReturnsAsync(@event.Id);
        mockRepository
            .Setup(repository => repository.GetByIdAsync(
                It.Is<Guid>(id => id == @event.Id), _ct))
            .ReturnsAsync(@event);

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.AddAsync(@event, _ct);

        // Assert
        Assert.NotNull(res);
        Assert.True(res.IsEqual(@event));
    }

    [Fact]
    public async Task GetAll_WithValidPagination_ReturnsPagedEvents()
    {
        // Arrange
        List<Event> events = EventFactory.CreateEvents(20);
        var expectedEvents = events.Skip(5).Take(5);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetPagedAsync(
                It.IsAny<IQueryable<Event>>(), 2, 5, _ct))
            .ReturnsAsync(new PagedEvents(expectedEvents, 5));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.GetAllAsync(
            new(null, null, null), new(2, 5), _ct);

        // Assert
        Assert.Equal(5, res.TotalCount);
        Assert.True(res.Items.First().IsEqual(expectedEvents.First()));
        Assert.True(res.Items.Last().IsEqual(expectedEvents.Last()));
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsCorrectEvent()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        Event @event = EventFactory.CreateEvent(guid);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository
                .GetByIdAsync(It.Is<Guid>(id => id == guid), _ct))
            .ReturnsAsync(@event);

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.GetByIdAsync(guid, _ct);

        // Assert
        Assert.NotNull(res);
        Assert.True(res.IsEqual(@event));
    }

    [Fact]
    public async Task Update_ExistingEvent_DoesNotThrowException()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        Event @event = EventFactory.CreateEvent(guid);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository
                .UpdateAsync(It.Is<Event>(e => e.Id == guid), _ct))
            .ReturnsAsync(true);

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = await Record
            .ExceptionAsync(() => eventService.UpdateAsync(@event, _ct));

        // Assert
        Assert.Null(exception);
        mockRepository
            .Verify(repo =>
                repo.UpdateAsync(It.IsAny<Event>(), _ct), Times.Once);
    }

    [Fact]
    public async Task Remove_ExistingId_DoesNotThrowException()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository
                .RemoveAsync(It.Is<Guid>(g => g == guid), _ct))
            .ReturnsAsync(true);

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = await Record
            .ExceptionAsync(() => eventService.RemoveAsync(guid, _ct));

        // Assert
        Assert.Null(exception);
        mockRepository
            .Verify(repo =>
                repo.RemoveAsync(It.IsAny<Guid>(), _ct), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithTitleFilter_ReturnsMatchingEvents()
    {
        // Arrange
        string filterTitle = "Title #1";

        List<Event> events = EventFactory.CreateEvents(10);
        var expectedEvents = events.Where(e => e.Title == filterTitle);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetPagedAsync(
                It.IsAny<IQueryable<Event>>(), 1, 10, _ct))
            .ReturnsAsync(new PagedEvents(expectedEvents, 1));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.GetAllAsync(
            new(filterTitle, null, null), new(1, 10), _ct);

        // Assert
        Assert.Equal(1, res.TotalCount);
        Assert.True(res.Items.First().IsEqual(expectedEvents.ElementAt(0)));
    }

    [Fact]
    public async Task GetAll_WithDateRangeFilter_ReturnsEventsWithinRange()
    {
        // Arrange
        DateTime invalidStartAt = DateTime.Now.AddDays(-5);
        DateTime invalidEndAt = DateTime.Now.AddDays(-4);
        DateTime validStartAt = DateTime.Now.AddDays(-3);
        DateTime validEndAt = DateTime.Now.AddDays(-1);

        // Arrange
        List<Event> events = [
            EventFactory.CreateEvent(
                null, null, invalidStartAt, invalidEndAt),
            EventFactory.CreateEvent(
                null, null, validStartAt, validEndAt),
            EventFactory.CreateEvent(
                null, null, validStartAt, validEndAt),
        ];

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetPagedAsync(
                It.IsAny<IQueryable<Event>>(), 1, 10, _ct))
            .ReturnsAsync(new PagedEvents(events.Skip(1).Take(2), 2));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.GetAllAsync(
            new(null, validStartAt, validEndAt), new(1, 10), _ct);

        // Assert
        Assert.Equal(2, res.TotalCount);
        Assert.True(res.Items.First().IsEqual(events.ElementAt(1)));
        Assert.True(res.Items.Last().IsEqual(events.ElementAt(2)));
    }

    [Fact]
    public async Task GetAll_WithTitleAndDateRangeFilter_ReturnsMatchingEventsWithinRange()
    {
        // Arrange
        string filterTitle = "The Bohemians";

        DateTime invalidStartAt = DateTime.Now.AddDays(-5);
        DateTime invalidEndAt = DateTime.Now.AddDays(-4);
        DateTime validStartAt = DateTime.Now.AddDays(-3);
        DateTime validEndAt = DateTime.Now.AddDays(-1);

        List<Event> events = [
            EventFactory.CreateEvent(
                null, "The Bohemians part #1", invalidStartAt, invalidEndAt),
            EventFactory.CreateEvent(
                null, "The Bohemians part #2", validStartAt, validEndAt),
            EventFactory.CreateEvent(
                null, "The Bohemians part #3", validStartAt, validEndAt),
            EventFactory.CreateEvent(
                null, "Dracula By Bram Stoker", validStartAt, validEndAt)
        ];

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository
                .GetPagedAsync(It.IsAny<IQueryable<Event>>(), 1, 10, _ct))
            .ReturnsAsync(new PagedEvents(events.Skip(1).Take(2), 2));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.GetAllAsync(
            new(filterTitle, validStartAt, validEndAt), new(1, 10), _ct);

        // Assert
        Assert.Equal(2, res.TotalCount);
        Assert.True(res.Items.First().IsEqual(events.ElementAt(1)));
        Assert.True(res.Items.Last().IsEqual(events.ElementAt(2)));
    }

    [Fact]
    public async Task GetById_NonExistentId_ThrowsEventNotFoundException()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetByIdAsync(guid, _ct))
            .Throws(new EventNotFoundException(guid));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = await Record
            .ExceptionAsync(() => eventService.GetByIdAsync(guid, _ct));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(exception.Message, $"Event with Id '{guid}' was not found.");
    }

    [Fact]
    public async Task Update_NonExistentId_ThrowsEventNotFoundException()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        Event @event = EventFactory.CreateEvent(guid);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.UpdateAsync(@event, _ct))
            .Throws(new EventNotFoundException(guid));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = await Record
            .ExceptionAsync(() => eventService.UpdateAsync(@event, _ct));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(exception.Message, $"Event with Id '{guid}' was not found.");
    }

    [Fact]
    public async Task Remove_NonExistentId_ThrowsEventNotFoundException()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        Event @event = EventFactory.CreateEvent(guid);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.RemoveAsync(guid, _ct))
            .Throws(new EventNotFoundException(guid));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = await Record
            .ExceptionAsync(() => eventService.RemoveAsync(guid, _ct));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(exception.Message, $"Event with Id '{guid}' was not found.");
    }
}
