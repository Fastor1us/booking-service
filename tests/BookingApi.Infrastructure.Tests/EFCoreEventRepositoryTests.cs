using BookingApi.Application.Interfaces;
using BookingApi.Domain.Constants;
using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Repositories;
using BookingApi.Infrastructure.Tests.Base;
using BookingApi.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Infrastructure.Tests;

public class EFCoreEventRepositoryTests : PostgreSqlBase
{
    #region Add
    [Fact]
    public async Task Add_ExistedEventId_ThrowsDbUpdateException()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var event1 = EventFactory.Generate();
        var event2 = EventFactory.Generate(event1.Id);
        eventRepository.Add(event1);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        eventRepository.Add(event1);
        var exception = await Record.ExceptionAsync(() =>
            context.SaveChangesAsync());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<DbUpdateException>(exception);
    }

    [Fact]
    public async Task Add_MultipleEvents_ShouldAddAll()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var events = EventFactory.Generate(5);
        foreach (var e in events)
        {
            eventRepository.Add(e);
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var allEvents = await eventRepository.ToListAsync(
            eventRepository
                .GetQuery(QueryTrackerBehavior.NoTracking));

        // Assert
        Assert.Equal(5, allEvents.Count);
    }
    #endregion

    #region FirstOrDefaultAsync
    [Fact]
    public async Task FirstOrDefaultAsync_ValidEventId_ReturnsCorrectEvent()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var createdEvent = await eventRepository
            .FirstOrDefaultAsync(e => e.Id == @event.Id);

        // Assert
        Assert.NotNull(createdEvent);
        Assert.Equal(@event.Id, createdEvent.Id);
        Assert.Equal(@event.Title, createdEvent.Title);
        Assert.Equal(@event.TotalSeats, createdEvent.TotalSeats);
        Assert.Equal(@event.TotalSeats, createdEvent.AvailableSeats);
        Assert.Equal(@event.StartAt, createdEvent.StartAt, TimeSpan.FromMicroseconds(1));
        Assert.Equal(@event.EndAt, createdEvent.EndAt, TimeSpan.FromMicroseconds(1));
    }

    [Fact]
    public async Task FirstOrDefaultAsync_NonExistentEventId_ReturnsNull()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var nonExistentEventId = Guid.NewGuid();

        // Act
        var createdEvent = await eventRepository
            .FirstOrDefaultAsync(e => e.Id == nonExistentEventId);

        // Assert
        Assert.Null(createdEvent);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithTrack_ShouldTrackEntity()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var trackedEvent = await eventRepository
            .FirstOrDefaultAsync(QueryTrackerBehavior.Track, e => e.Id == @event.Id);
        trackedEvent!.AvailableSeats--;
        context.SaveChanges();
        context.ChangeTracker.Clear();
        var actualEvent = await eventRepository
            .FirstOrDefaultAsync(e => e.Id == @event.Id);

        // Assert
        Assert.Equal(@event.AvailableSeats - 1, actualEvent!.AvailableSeats);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithNoTracking_ShouldNotTrackEntity()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var noTrackedEvent = await eventRepository
            .FirstOrDefaultAsync(QueryTrackerBehavior.NoTracking, e => e.Id == @event.Id);
        noTrackedEvent!.AvailableSeats--;
        context.SaveChanges();
        context.ChangeTracker.Clear();
        var actualEvent = await eventRepository
            .FirstOrDefaultAsync(e => e.Id == @event.Id);

        // Assert
        Assert.Equal(@event.AvailableSeats, actualEvent!.AvailableSeats);
    }
    #endregion

    #region ExecuteUpdateByIdAsync
    [Fact]
    public async Task ExecuteUpdateByIdAsync_ValidEventId_ReturnsUpdatedEvent()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var updatedCount = await eventRepository.ExecuteUpdateByIdAsync(new()
        {
            Id = @event.Id,
            Title = "NewTitle",
            TotalSeats = EventConstants.MinTotalSeats,
            Description = "NewDescription",
            StartAt = @event.StartAt.AddHours(1),
            EndAt = @event.EndAt.AddHours(1)
        });
        var updatedEvent = await eventRepository
            .FirstOrDefaultAsync(e => e.Id == @event.Id);

        // Assert
        Assert.Equal(1, updatedCount);
        Assert.NotNull(updatedEvent);
        Assert.Equal(@event.Id, updatedEvent.Id);
        Assert.Equal("NewTitle", updatedEvent.Title);
        Assert.Equal("NewDescription", updatedEvent.Description);
        Assert.Equal(
            @event.StartAt.AddHours(1),
            updatedEvent.StartAt,
            TimeSpan.FromMicroseconds(1));
        Assert.Equal(
            @event.EndAt.AddHours(1),
            updatedEvent.EndAt,
            TimeSpan.FromMicroseconds(1));
    }
    #endregion

    #region ExecuteDeleteByIdAsync
    [Fact]
    public async Task ExecuteDeleteByIdAsync_ValidEventId_RemovesCorrectEvent()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var deletedCount = await eventRepository
            .ExecuteDeleteByIdAsync(@event.Id);
        context.ChangeTracker.Clear();
        int removedEvent = await eventRepository
            .ExecuteDeleteByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(1, deletedCount);
        Assert.Equal(0, removedEvent);
    }

    [Fact]
    public async Task ExecuteDeleteByIdAsync_NonExistentEventId_RemovesCorrectEvent()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);

        // Act
        int removedEvent = await eventRepository
            .ExecuteDeleteByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(0, removedEvent);
    }
    #endregion

    #region GetQuery
    [Fact]
    public async Task GetQuery_WithTracking_ShouldTrackEntities()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var query = eventRepository.GetQuery(QueryTrackerBehavior.Track);
        var retrievedEvent = await eventRepository
            .FirstOrDefaultAsync(query, e => e.Id == @event.Id);
        retrievedEvent!.Title = "Modified Title";
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Assert
        var updatedEvent = await eventRepository
            .FirstOrDefaultAsync(query, e => e.Id == @event.Id);

        Assert.Equal("Modified Title", updatedEvent!.Title);
    }

    [Fact]
    public async Task GetQuery_WithNoTracking_ShouldNotTrackEntities()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var query = eventRepository.GetQuery(QueryTrackerBehavior.NoTracking);
        var retrievedEvent = await eventRepository
            .FirstOrDefaultAsync(query, e => e.Id == @event.Id);
        retrievedEvent!.Title = "Modified Title";
        await context.SaveChangesAsync();
        var unchangedEvent = await eventRepository
            .FirstOrDefaultAsync(e => e.Id == @event.Id);

        // Assert
        Assert.Equal(@event.Title, unchangedEvent!.Title);
        Assert.NotEqual("Modified Title", unchangedEvent.Title);
    }

    [Fact]
    public async Task GetQuery_WithComplexFiltering_ShouldReturnCorrectResult()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);

        var now = DateTimeOffset.UtcNow;
        var events = EventFactory.Generate(20);

        foreach (var e in events)
        {
            eventRepository.Add(e);
        }
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var filteredEvents = await eventRepository.ToListAsync(
            eventRepository
                .GetQuery(QueryTrackerBehavior.NoTracking)
                .Where(e => e.StartAt > now.AddDays(-5))
                .OrderBy(e => e.StartAt));

        // Assert
        Assert.Equal(6, filteredEvents.Count);
        Assert.Equal("Title #15", filteredEvents[0].Title);
    }

    [Fact]
    public async Task GetQuery_WithPagination_ShouldReturnCorrectResult()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);

        var now = DateTimeOffset.UtcNow;
        var events = EventFactory.Generate(20);

        foreach (var e in events)
        {
            eventRepository.Add(e);
        }
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var filteredEvents = await eventRepository.ToListAsync(
            eventRepository
                .GetQuery(QueryTrackerBehavior.NoTracking)
                .OrderBy(e => e.StartAt)
                .Skip(5)
                .Take(5));

        // Assert
        Assert.Equal(5, filteredEvents.Count);
        Assert.Equal("Title #6", filteredEvents[0].Title);
    }
    #endregion

    #region CountAsync
    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);

        var events = EventFactory.Generate(5);

        foreach (var e in events)
        {
            eventRepository.Add(e);
        }
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var query = eventRepository.GetQuery(QueryTrackerBehavior.NoTracking);
        var count = await eventRepository.CountAsync(query);

        // Assert
        Assert.Equal(5, count);
    }
    #endregion
}
