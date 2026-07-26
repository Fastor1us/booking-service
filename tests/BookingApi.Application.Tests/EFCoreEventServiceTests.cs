using BookingApi.Application.Dtos;
using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using BookingApi.Domain.Exceptions;
using BookingApi.Infrastructure.Persistence;
using BookingApi.Infrastructure.Repositories;
using BookingApi.Infrastructure.UnitOfWork;
using BookingApi.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApi.Application.Tests;

public class EFCoreEventServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IEventService _eventService;
    private readonly CancellationToken _ct;

    public EFCoreEventServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IEventRepository, EFCoreEventRepository>();
        services.AddScoped<IBookingRepository, EFCoreBookingRepository>();
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

        services.AddScoped<IEventService, EventService>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();

        var context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        _ct = CancellationToken.None;
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _serviceProvider?.Dispose();
    }

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
