using BookingApi.Application.Dtos;
using BookingApi.Application.Tests.Helpers;
using BookingApi.Domain.Constants;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Tests;

public class EFCoreBookingServiceTests : EFCoreServiceTestsBase
{
    #region GetByIdAsync
    [Fact]
    public async Task GetByIdAsync_ValidBookingId_ReturnsCorrectBooking()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);
        var createdBooking = await _bookingService.AddAsync(addedEvent.Id, user.Login, _ct);

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
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);

        // Act
        var result = await _bookingService.AddAsync(addedEvent.Id, user.Login, _ct);

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
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);

        // Act
        await _bookingService.AddAsync(addedEvent.Id, user.Login, _ct);

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
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);
        await _bookingService.AddAsync(addedEvent.Id, user.Login, _ct);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _bookingService.AddAsync(addedEvent.Id, user.Login, _ct));

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
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _bookingService.AddAsync(nonExistentEventId, user.Login, _ct));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task CreateAsync_PastEvent_ThrowsBookingPastEventException()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>(startAt: DateTime.Now.AddDays(-1));
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var expectedException = new BookingPastEventException(addedEvent.Id);
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _bookingService.AddAsync(addedEvent.Id, user.Login, _ct));

        // Assert
        Assert.IsType<BookingPastEventException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ExceedMaxActiveBookingsUserLimit_ThrowsBookingPastEventException()
    {
        // Arrange
        var maxActiveBookings = UserConstant.MaxActiveBookings;
        var createEventDto = EventFactory.Generate<CreateEventDto>(totalSeats: maxActiveBookings + 1);
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);
        var expectedException = new BookingExceedLimitException(user.Login);
        for (int i = 0; i < maxActiveBookings; i++)
        {
            await _bookingService.AddAsync(addedEvent.Id, user.Login, _ct);
        }

        // Act
        var exception = await Record.ExceptionAsync(
            () => _bookingService.AddAsync(addedEvent.Id, user.Login, _ct));

        // Assert
        Assert.IsType<BookingExceedLimitException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task CreateAsync_EveryUserHasItsOwnMaxActiveBookingsLimit()
    {
        // Arrange
        var maxActiveBookings = UserConstant.MaxActiveBookings;
        var createEventDto = EventFactory.Generate<CreateEventDto>(totalSeats: maxActiveBookings + 1);
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var user1 = await _userService.RegisterAsync(UserFactory.GenerateCreateDto("user1"), _ct);
        var user2 = await _userService.RegisterAsync(UserFactory.GenerateCreateDto("user2"), _ct);
        var expectedException = new BookingExceedLimitException(user1.Login);
        for (int i = 0; i < maxActiveBookings; i++)
        {
            await _bookingService.AddAsync(addedEvent.Id, user1.Login, _ct);
        }

        // Act
        var exception = await Record.ExceptionAsync(
            () => _bookingService.AddAsync(addedEvent.Id, user1.Login, _ct));
        var user2Booking = await _bookingService.AddAsync(addedEvent.Id, user2.Login, _ct);

        // Assert
        Assert.IsType<BookingExceedLimitException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
        Assert.NotNull(user2Booking);
        Assert.Equal(addedEvent.Id, user2Booking.EventId);
    }

    [Fact]
    public async Task CreateAsync_ConcurrentCallsWithSameValidEventId_CreatesAllBookings()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>(totalSeats: 3);
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var eventId = addedEvent.Id;
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);

        // Act
        var tasks = new List<Task<Booking>>();
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(_bookingService.AddAsync(eventId, user.Login, _ct));
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
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);

        // Act
        var tasks = new List<Task<Booking>>();
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(_bookingService.AddAsync(eventId, user.Login, _ct));
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
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);

        // Act
        await _bookingService.AddAsync(event1.Id, user.Login, _ct);
        await _bookingService.AddAsync(event1.Id, user.Login, _ct);
        await _bookingService.AddAsync(event2.Id, user.Login, _ct);
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
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);

        // Act
        var result = await _bookingService.AddAsync(addedEvent.Id, user.Login, _ct);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(result.CreatedAt >= beforeCreation);
        Assert.True(result.CreatedAt <= afterCreation);
    }
    #endregion

    #region CancelAsync
    [Fact]
    public async Task CancelAsync_OwnBooking_ShouldCancelSuccessfully()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);
        var booking = await _bookingService.AddAsync(addedEvent.Id, user.Login, _ct);

        // Act
        await _bookingService.CancelAsync(booking.Id, user.Login, _ct);
        var cancelledBooking = await _bookingService.GetByIdAsync(booking.Id, _ct);

        // Assert
        Assert.Equal(BookingStatus.Cancelled, cancelledBooking.Status);
    }

    [Fact]
    public async Task CancelAsync_AdminCancellingOtherUsersBooking_ShouldCancelSuccessfully()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);
        var admin = await _userService.RegisterAsync(
            UserFactory.GenerateCreateDto("admin", "admin", UserRole.Admin), _ct);
        var booking = await _bookingService.AddAsync(addedEvent.Id, user.Login, _ct);

        // Act
        await _bookingService.CancelAsync(booking.Id, admin.Login, _ct);
        var cancelledBooking = await _bookingService.GetByIdAsync(booking.Id, _ct);

        // Assert
        Assert.Equal(BookingStatus.Cancelled, cancelledBooking.Status);
    }

    [Fact]
    public async Task CancelAsync_UserTryCancelOtherUsersBooking_ThrowsBookingUnauthorizedCancelException()
    {
        // Arrange
        var createEventDto = EventFactory.Generate<CreateEventDto>();
        var addedEvent = await _eventService.AddAsync(createEventDto, _ct);
        var user1 = await _userService.RegisterAsync(UserFactory.GenerateCreateDto("user1"), _ct);
        var user2 = await _userService.RegisterAsync(UserFactory.GenerateCreateDto("user2"), _ct);
        var booking = await _bookingService.AddAsync(addedEvent.Id, user1.Login, _ct);
        var expectedException = new ForbiddenException();

        // Act
        var exception = await Record.ExceptionAsync(
            () => _bookingService.CancelAsync(booking.Id, user2.Login, _ct));
        var unchangedBooking = await _bookingService.GetByIdAsync(booking.Id, _ct);

        // Assert
        Assert.IsType<ForbiddenException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
        Assert.Equal(BookingStatus.Pending, unchangedBooking.Status);
    }

    [Fact]
    public async Task CancelAsync_NonExistentBooking_ThrowsBookingNotFoundException()
    {
        // Arrange
        var nonExistentBookingId = Guid.NewGuid();
        var user = await _userService.RegisterAsync(UserFactory.GenerateCreateDto(), _ct);
        var expectedException = new BookingNotFoundException(nonExistentBookingId);

        // Act
        var exception = await Record.ExceptionAsync(
            () => _bookingService.CancelAsync(nonExistentBookingId, user.Login, _ct));

        // Assert
        Assert.IsType<BookingNotFoundException>(exception);
        Assert.Equal(expectedException.Message, exception.Message);
    }
    #endregion
}
