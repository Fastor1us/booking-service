using EventService.Application.Cache;
using EventService.Application.Dtos;
using EventService.Application.Interfaces;
using EventService.Application.Tests.Helpes;
using EventService.Domain.Exceptions;
using EventService.Domain.Models;
using Messaging.Abstractions.Persistence;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq.Expressions;

namespace EventService.Application.Tests;

public class EventServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IEventCache> _cache = new();

    private readonly EventCacheOptions _options = new()
    {
        EventTtl = TimeSpan.FromMinutes(5),
        TopEventsTtl = TimeSpan.FromMinutes(1)
    };

    public EventServiceTests()
    {
        _uow.SetupGet(u => u.Events).Returns(_events.Object);
    }

    #region CacheMiss

    [Fact]
    public async Task GetByIdAsync_WhenCacheMiss_InvokesFactory_AndReturnsFromRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var @event = EventFactory.Generate(id);

        SetupCacheMiss<Event>(EventCacheKey.ForId(id), _options.EventTtl);

        _events
            .Setup(e => e.FirstOrDefaultAsync(
                QueryTrackerBehavior.NoTracking,
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(@event);

        var es = CreateEventService();

        // Act
        var result = await es.GetByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.True(result.IsEqual(@event));

        _events.Verify(
            e => e.FirstOrDefaultAsync(
                QueryTrackerBehavior.NoTracking,
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCacheMiss_AndEntityMissing_ThrowsEventNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedException = new EventNotFoundException(id);

        SetupCacheMiss<Event>(EventCacheKey.ForId(id), _options.EventTtl);

        _events
            .Setup(e => e.FirstOrDefaultAsync(
                QueryTrackerBehavior.NoTracking,
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var es = CreateEventService();

        // Act
        var exception = await Record.ExceptionAsync(
            () => es.GetByIdAsync(id, CancellationToken.None));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task GetTopAsync_WhenCacheMiss_InvokesFactory_AndReturnsFromRepository()
    {
        // Arrange
        var @event = EventFactory.Generate(5);

        SetupCacheMiss<List<Event>>(EventCacheKey.Top10, _options.TopEventsTtl);

        _events
            .Setup(e => e.GetQuery(It.IsAny<QueryTrackerBehavior>()))
            .Returns(@event.AsQueryable());

        _events
            .Setup(e => e.ToListAsync(
                It.IsAny<IQueryable<Event>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(@event);

        var es = CreateEventService();

        // Act
        var result = await es.GetTopAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(@event.Count, result.Count);

        _events.Verify(
            e => e.ToListAsync(
                It.IsAny<IQueryable<Event>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region CacheHit

    [Fact]
    public async Task GetByIdAsync_WhenCacheHit_ReturnsCachedEvent_AndDoesNotTouchRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cached = EventFactory.Generate(id, title: "cached");

        SetupCacheHit(EventCacheKey.ForId(id), _options.EventTtl, cached);

        var es = CreateEventService();

        // Act
        var result = await es.GetByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.True(result.IsEqual(cached));

        _events.Verify(
            e => e.FirstOrDefaultAsync(
                It.IsAny<QueryTrackerBehavior>(),
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTopAsync_WhenCacheHit_ReturnsCachedList_AndDoesNotTouchRepository()
    {
        // Arrange
        var cached = EventFactory.Generate(3);

        SetupCacheHit(EventCacheKey.Top10, _options.TopEventsTtl, cached);

        var es = CreateEventService();

        // Act
        var result = await es.GetTopAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cached.Count, result.Count);

        _events.Verify(
            e => e.ToListAsync(
                It.IsAny<IQueryable<Event>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _events.Verify(
            e => e.GetQuery(It.IsAny<QueryTrackerBehavior>()),
            Times.Never);
    }

    #endregion

    #region InvalidatingCache

    [Fact]
    public async Task AddAsync_SetsCacheWithEventKeyAndTtl()
    {
        // Arrange
        var dto = EventFactory.Generate<CreateEventDto>();
        var es = CreateEventService();

        // Act
        var created = await es.AddAsync(dto, CancellationToken.None);

        // Assert
        _events.Verify(e => e.Add(It.Is<Event>(x => x.Id == created.Id)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _cache.Verify(c => c.SetAsync(
            EventCacheKey.ForId(created.Id),
            created,
            _options.EventTtl), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUpdated_InvalidatesCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = EventFactory.Generate<UpdateEventDto>();

        _events
            .Setup(e => e.ExecuteUpdateByIdAsync(
                It.IsAny<Event>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var es = CreateEventService();

        // Act
        await es.UpdateAsync(id, dto, CancellationToken.None);

        // Assert
        _cache.Verify(c => c.RemoveAsync(
            EventCacheKey.ForId(id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNothingUpdated_DoesNotInvalidateCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = EventFactory.Generate<UpdateEventDto>();

        _events
            .Setup(e => e.ExecuteUpdateByIdAsync(
                It.IsAny<Event>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var es = CreateEventService();

        // Act
        await es.UpdateAsync(id, dto, CancellationToken.None);

        // Assert
        _cache.Verify(c => c.RemoveAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_WhenDeleted_InvalidatesCache()
    {
        // Arrange
        var id = Guid.NewGuid();

        _events
            .Setup(e => e.ExecuteDeleteByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var es = CreateEventService();

        // Act
        await es.RemoveAsync(id, CancellationToken.None);

        // Assert
        _cache.Verify(c => c.RemoveAsync(
            EventCacheKey.ForId(id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WhenNotFound_Throws_AndDoesNotTouchCache()
    {
        // Arrange
        var id = Guid.NewGuid();

        _events
            .Setup(e => e.ExecuteDeleteByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var es = CreateEventService();

        // Act + Assert
        await Assert.ThrowsAsync<EventNotFoundException>(
            () => es.RemoveAsync(id, CancellationToken.None));

        _cache.Verify(c => c.RemoveAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Helpers

    private Services.EventService CreateEventService() =>
        new(_uow.Object, _cache.Object, Options.Create(_options));

    private void SetupCacheMiss<T>(string key, TimeSpan ttl) where T : class?
    {
        _cache
            .Setup(c => c.GetOrSetAsync(
                key,
                It.IsAny<Func<Task<T>>>(),
                ttl,
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<T>>, TimeSpan, CancellationToken>(
                (_, factory, _, _) => factory());
    }

    private void SetupCacheHit<T>(string key, TimeSpan ttl, T value) where T : class?
    {
        _cache
            .Setup(c => c.GetOrSetAsync(
                key,
                It.IsAny<Func<Task<T>>>(),
                ttl,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(value);
    }

    #endregion
}