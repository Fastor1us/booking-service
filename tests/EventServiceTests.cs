using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
using BookingTests.Helpers;
using Moq;

namespace BookingTests;

public class EventServiceTests
{
    [Fact]
    public async Task Add_ValidEvent_ReturnsAddedEvent()
    {
        // Arrange
        Event @event = EventFactory.CreateEvent();

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.Add(It.IsAny<Event>()))
            .ReturnsAsync(@event.Id);
        mockRepository
            .Setup(repository => repository.GetById(
                It.Is<Guid>(e => e == @event.Id)))
            .ReturnsAsync(@event);

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.Add(@event);

        // Assert
        Assert.NotNull(res);
        AssertEventsEqual(@event, res);
    }

    [Fact]
    public async Task GetAll_WithValidPagination_ReturnsPagedEvents()
    {
        // Arrange
        List<Event> events = EventFactory.CreateEvents(20);
        var expectedEvents = events.Skip(5).Take(5);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetPaged(
                It.IsAny<IQueryable<Event>>(), 2, 5))
            .ReturnsAsync(new PagedEvents(expectedEvents, 5));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.GetAll(new(null, null, null), new(2, 5));

        // Assert
        Assert.Equal(5, res.TotalCount);
        AssertEventsEqual(expectedEvents.First(), res.Items.First());
        AssertEventsEqual(expectedEvents.Last(), res.Items.Last());
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsCorrectEvent()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        Event @event = EventFactory.CreateEvent(guid);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetById(It.Is<Guid>(e => e == guid)))
            .ReturnsAsync(@event);

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.GetById(guid);

        // Assert
        Assert.NotNull(res);
        AssertEventsEqual(@event, res);
    }

    [Fact]
    public async Task Update_ExistingEvent_DoesNotThrowException()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        Event @event = EventFactory.CreateEvent(guid);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.Update(It.Is<Event>(e => e.Id == guid)))
            .Verifiable();

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => eventService.Update(@event));

        // Assert
        Assert.Null(exception);
        mockRepository
            .Verify(repo => repo.Update(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public async Task Remove_ExistingId_DoesNotThrowException()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.Remove(It.Is<Guid>(g => g == guid)))
            .Verifiable();

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => eventService.Remove(guid));

        // Assert
        Assert.Null(exception);
        mockRepository
            .Verify(repo => repo.Remove(It.IsAny<Guid>()), Times.Once);
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
            .Setup(repository => repository.GetPaged(
                It.IsAny<IQueryable<Event>>(), 1, 10))
            .ReturnsAsync(new PagedEvents(expectedEvents, 1));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.GetAll(
            new(filterTitle, null, null), new(1, 10));

        // Assert
        Assert.Equal(1, res.TotalCount);
        AssertEventsEqual(expectedEvents.ElementAt(0), res.Items.First());
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
            .Setup(repository => repository.GetPaged(
                It.IsAny<IQueryable<Event>>(), 1, 10))
            .ReturnsAsync(new PagedEvents(events.Skip(1).Take(2), 2));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.GetAll(
            new(null, validStartAt, validEndAt),
            new(1, 10));

        // Assert
        Assert.Equal(2, res.TotalCount);
        AssertEventsEqual(events.ElementAt(1), res.Items.First());
        AssertEventsEqual(events.ElementAt(2), res.Items.Last());
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
            .Setup(repository => repository.GetPaged(It.IsAny<IQueryable<Event>>(), 1, 10))
            .ReturnsAsync(new PagedEvents(events.Skip(1).Take(2), 2));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = await eventService.GetAll(
            new(filterTitle, validStartAt, validEndAt),
            new(1, 10));

        // Assert
        Assert.Equal(2, res.TotalCount);
        AssertEventsEqual(events.ElementAt(1), res.Items.First());
        AssertEventsEqual(events.ElementAt(2), res.Items.Last());
    }

    [Fact]
    public async Task GetById_NonExistentId_ThrowsEventNotFoundException()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetById(guid))
            .Throws(new EventNotFoundException(guid));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => eventService.GetById(guid));

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
            .Setup(repository => repository.Update(@event))
            .Throws(new EventNotFoundException(guid));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => eventService.Update(@event));

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
            .Setup(repository => repository.Remove(guid))
            .Throws(new EventNotFoundException(guid));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => eventService.Remove(guid));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(exception.Message, $"Event with Id '{guid}' was not found.");
    }

    #region Helper Methods
    private static void AssertEventsEqual(Event expected, Event actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.StartAt, actual.StartAt);
        Assert.Equal(expected.EndAt, actual.EndAt);
    }
    #endregion
}
