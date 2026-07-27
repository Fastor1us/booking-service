using BookingApi.Application.Dtos;
using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using BookingApi.Application.Tests.Helpers;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Persistence;
using BookingApi.Infrastructure.Repositories;
using BookingApi.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApi.Application.Tests;

public class EFCoreBookingServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IBookingService _bookingService;
    private readonly IEventService _eventService;
    private readonly CancellationToken _ct;

    public EFCoreBookingServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IEventRepository, EFCoreEventRepository>();
        services.AddScoped<IBookingRepository, EFCoreBookingRepository>();
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();

        var context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
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
    public async Task GetByIdAsync_ValidBookingId_ReturnsCorrectBooking()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var createdBooking = await _bookingService.AddAsync(addedEvent.Id, _ct);

        // Act
        var result = await _bookingService.GetByIdAsync(createdBooking.Id, _ct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdBooking.Id, result.Id);
        Assert.Equal(createdBooking.EventId, result.EventId);
        Assert.Equal(createdBooking.Status, result.Status);
        Assert.Equal(createdBooking.CreatedAt, result.CreatedAt);
        Assert.Equal(createdBooking.ProcessedAt, result.ProcessedAt);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentBookingId_ThrowsBookingNotFoundException()
    {
        // Arrange
        var nonExistentBookingId = Guid.NewGuid();
        var expectedException = new BookingNotFoundException(nonExistentBookingId);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _bookingService.GetByIdAsync(nonExistentBookingId, _ct));

        // Assert
        Assert.IsType<BookingNotFoundException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }
    #endregion

    #region CreateAsync
    [Fact]
    public async Task CreateAsync_ValidEventId_ReturnsCreatedBooking()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);

        // Act
        var result = await _bookingService.AddAsync(addedEvent.Id, _ct);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(addedEvent.Id, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateAsync_ValidEventId_DecreasesAvailableSeats()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>(totalSeats: 5);
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var initialAvailableSeats = addedEvent.AvailableSeats;

        // Act
        await _bookingService.AddAsync(addedEvent.Id, _ct);

        // Assert
        var updatedEvent = await _eventService.GetByIdAsync(addedEvent.Id, _ct);
        Assert.Equal(initialAvailableSeats - 1, updatedEvent.AvailableSeats);
    }

    [Fact]
    public async Task CreateAsync_WithNoAvailableSeats_ThrowsNoAvailableSeatsException()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>(totalSeats: 1);
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var expectedException = new NoAvailableSeatsException(addedEvent.Id);
        await _bookingService.AddAsync(addedEvent.Id, _ct);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _bookingService.AddAsync(addedEvent.Id, _ct));

        // Assert
        Assert.IsType<NoAvailableSeatsException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task CreateAsync_NonExistentEventId_ThrowsEventNotFoundException()
    {
        // Arrange
        var nonExistentEventId = Guid.NewGuid();
        var expectedException = new EventNotFoundException(nonExistentEventId);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _bookingService.AddAsync(nonExistentEventId, _ct));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ConcurrentCallsWithSameValidEventId_CreatesAllBookings()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>(totalSeats: 3);
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var eventId = addedEvent.Id;

        // Act
        var tasks = new List<Task<Booking>>();
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(_bookingService.AddAsync(eventId, _ct));
        }
        var results = await Task.WhenAll(tasks);
        var updatedEvent = await _eventService.GetByIdAsync(eventId, _ct);

        // Assert
        Assert.Equal(3, results.Length);
        Assert.All(results, booking =>
        {
            Assert.NotNull(booking);
            Assert.Equal(eventId, booking.EventId);
        });
        Assert.Equal(3, results.Select(b => b.Id).Distinct().Count());
        Assert.Equal(0, updatedEvent.AvailableSeats);
    }

    [Fact]
    public async Task CreateAsync_ConcurrentCallsWithMoreThanAvailableSeats_ThrowsException()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>(totalSeats: 2);
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var eventId = addedEvent.Id;

        // Act
        var tasks = new List<Task<Booking>>();
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(_bookingService.AddAsync(eventId, _ct));
        }

        var exceptions = new List<Exception>();
        foreach (var task in tasks)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        var bookingsCount = tasks.Count(t => t.IsCompletedSuccessfully);

        // Assert
        Assert.Contains(exceptions, ex => ex is NoAvailableSeatsException);
        Assert.Equal(2, bookingsCount);
    }

    [Fact]
    public async Task CreateAsync_WithDifferentEvents_DoesNotAffectEachOther()
    {
        // Arrange
        var createEventDto1 = EventFactory.Generate<CreateEventDto>(totalSeats: 3);
        var createEventDto2 = EventFactory.Generate<CreateEventDto>(totalSeats: 5);
        var event1 = await _eventService.AddAsync(createEventDto1, _ct);
        var event2 = await _eventService.AddAsync(createEventDto2, _ct);

        // Act
        await _bookingService.AddAsync(event1.Id, _ct);
        await _bookingService.AddAsync(event1.Id, _ct);
        await _bookingService.AddAsync(event2.Id, _ct);
        var updatedEvent1 = await _eventService.GetByIdAsync(event1.Id, _ct);
        var updatedEvent2 = await _eventService.GetByIdAsync(event2.Id, _ct);

        // Assert
        Assert.Equal(1, updatedEvent1.AvailableSeats);
        Assert.Equal(4, updatedEvent2.AvailableSeats);
    }

    [Fact]
    public async Task CreateAsync_BookingHasCorrectTimestamp()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);

        // Act
        var result = await _bookingService.AddAsync(addedEvent.Id, _ct);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(result.CreatedAt >= beforeCreation);
        Assert.True(result.CreatedAt <= afterCreation);
    }
    #endregion
}
