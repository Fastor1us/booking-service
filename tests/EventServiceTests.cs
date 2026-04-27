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
    public void Add_ReturnsAddedEvent()
    {
        // Arrange
        Event @event = EventFactory.CreateEvent();

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.Add(It.IsAny<Event>()))
            .Returns(@event.Id);
        mockRepository
            .Setup(repository => repository.GetById(
                It.Is<Guid>(e => e == @event.Id)))
            .Returns(@event);

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = eventService.Add(@event);

        // Assert
        Assert.NotNull(res);
        AssertEventsEqual(@event, res);
    }

    [Fact]
    public void GetAll_ReturnsPagedEvents()
    {
        // Arrange
        List<Event> events = EventFactory.CreateEvents(20);
        var expectedEvents = events.Skip(5).Take(5);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetPaged(
                It.IsAny<IQueryable<Event>>(), 2, 5))
            .Returns(new PagedEvents(expectedEvents, 5));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = eventService.GetAll(new(null, null, null), new(2, 5));

        // Assert
        Assert.Equal(5, res.TotalCount);
        AssertEventsEqual(expectedEvents.First(), res.Items.First());
        AssertEventsEqual(expectedEvents.Last(), res.Items.Last());
    }

    [Fact]
    public void GetById_ReturnsCorrentEvent()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        Event @event = EventFactory.CreateEvent(guid);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetById(It.Is<Guid>(e => e == guid)))
            .Returns(@event);

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = eventService.GetById(guid);

        // Assert
        Assert.NotNull(res);
        AssertEventsEqual(@event, res);
    }

    [Fact]
    public void Update_DoesNotThrowException()
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
        var exception = Record.Exception(() => eventService.Update(@event));

        // Assert
        Assert.Null(exception);
        mockRepository
            .Verify(repo => repo.Update(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public void Remove_DoesNotThrowException()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.Remove(It.Is<Guid>(g => g == guid)))
            .Verifiable();

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = Record.Exception(() => eventService.Remove(guid));

        // Assert
        Assert.Null(exception);
        mockRepository
            .Verify(repo => repo.Remove(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public void GetAll_ReturnsCorrectEvents_WhenFilterByTitle()
    {
        // Arrange
        string filterTitle = "Title #1";

        List<Event> events = EventFactory.CreateEvents(10);
        var expectedEvents = events.Where(e => e.Title == filterTitle);

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetPaged(
                It.IsAny<IQueryable<Event>>(), 1, 10))
            .Returns(new PagedEvents(expectedEvents, 1));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = eventService.GetAll(
            new(filterTitle, null, null), new(1, 10));

        // Assert
        Assert.Equal(1, res.TotalCount);
        AssertEventsEqual(expectedEvents.ElementAt(0), res.Items.First());
    }

    [Fact]
    public void GetAll_ReturnsCorrectEvents_WhenFilterByDates()
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
            .Returns(new PagedEvents(events.Skip(1).Take(2), 2));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = eventService.GetAll(
            new(null, validStartAt, validEndAt),
            new(1, 10));

        // Assert
        Assert.Equal(2, res.TotalCount);
        AssertEventsEqual(events.ElementAt(1), res.Items.First());
        AssertEventsEqual(events.ElementAt(2), res.Items.Last());
    }

    [Fact]
    public void GetAll_ReturnsCorrectEvents_WhenFilterByTitleAndDates()
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
            .Returns(new PagedEvents(events.Skip(1).Take(2), 2));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var res = eventService.GetAll(
            new(filterTitle, validStartAt, validEndAt),
            new(1, 10));

        // Assert
        Assert.Equal(2, res.TotalCount);
        AssertEventsEqual(events.ElementAt(1), res.Items.First());
        AssertEventsEqual(events.ElementAt(2), res.Items.Last());
    }

    [Fact]
    public void GetById_ThrowsError_WhenCallWithUnexistedId()
    {
        // Arrange
        Guid guid = Guid.NewGuid();

        var mockRepository = new Mock<IEventRepository>();
        mockRepository
            .Setup(repository => repository.GetById(guid))
            .Throws(new EventNotFoundException(guid));

        var eventService = new EventService(mockRepository.Object);

        // Act
        var exception = Record.Exception(() => eventService.GetById(guid));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(exception.Message, $"Event with Id '{guid}' was not found.");
    }

    [Fact]
    public void Update_ThrowsError_WhenCallWithUnexistedId()
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
        var exception = Record.Exception(() => eventService.Update(@event));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(exception.Message, $"Event with Id '{guid}' was not found.");
    }

    [Fact]
    public void Remove_ThrowsError_WhenCallWithUnexistedId()
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
        var exception = Record.Exception(() => eventService.Remove(guid));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(exception.Message, $"Event with Id '{guid}' was not found.");
    }

    #region 
    private static void AssertEventsEqual(Event expected, Event actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.StartAt, actual.StartAt);
        Assert.Equal(expected.EndAt, actual.EndAt);
    }
    #endregion
}
