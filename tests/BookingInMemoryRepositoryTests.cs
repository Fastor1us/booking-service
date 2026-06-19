using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Repositories;
using BookingTests.Helpers;
using Moq;

namespace BookingTests;

public class BookingInMemoryRepositoryTests
{
    private readonly CancellationToken _ct = CancellationToken.None;

    #region TryCreateBookingAsync

    [Fact]
    public async Task TryCreateBookingAsync_WithValidEventId_ShouldCreatePendingBooking()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository.Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);
        var bookingRepo = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act
        var result = await bookingRepo.TryCreateBookingAsync(@event.Id, _ct);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Booking);
        Assert.Equal(@event.Id, result.Booking.EventId);
        Assert.Equal(BookingStatus.Pending, result.Booking.Status);
        Assert.NotEqual(Guid.Empty, result.Booking.Id);
        Assert.NotEqual(default, result.Booking.CreatedAt);
        Assert.Null(result.Booking.ProcessedAt);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task TryCreateBookingAsync_WithValidEventId_ShouldDecrementsAvailableSeats()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        int expectedAvailableSeats = @event.AvailableSeats - 1;
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository.Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);
        var bookingRepo = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act
        var result = await bookingRepo.TryCreateBookingAsync(@event.Id, _ct);

        // Assert
        Assert.Equal(expectedAvailableSeats, @event.AvailableSeats);
    }

    [Fact]
    public async Task TryCreateBookingAsync_ConcurrentCallsWithSameValidEventId_ShouldDecrementsAvailableSeats()
    {
        // Arrange
        int amountOfCreatingBooking = 3;
        Event @event = EventFactory.Generate();
        int expectedAvailableSeats = @event.AvailableSeats - amountOfCreatingBooking;
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository.Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);
        var bookingRepo = new BookingInMemoryRepository(mockEventRepository.Object);

        var tasks = Enumerable
            .Range(0, amountOfCreatingBooking)
            .Select(_ => bookingRepo.TryCreateBookingAsync(@event.Id, _ct));

        // Act
        var result = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(expectedAvailableSeats, @event.AvailableSeats);
    }

    [Fact]
    public async Task TryCreateBookingAsync_ConcurrentCallsWithSameValidEventId_ShouldCreatePendingBookingsDependsOnAvailableSeatsCount()
    {
        // Arrange
        Guid eventId = Guid.NewGuid();
        int availableSeats = 5;
        int amountOfCreatingBooking = 20;
        int expectedAvailableSeats = availableSeats <= amountOfCreatingBooking
            ? 0
            : availableSeats - amountOfCreatingBooking;

        Event @event = EventFactory.Generate(
            eventId,
            "Title",
            DateTime.Now.AddDays(-1),
            DateTime.Now,
            availableSeats,
            availableSeats);

        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository.Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);
        var bookingRepo = new BookingInMemoryRepository(mockEventRepository.Object);

        var tasks = Enumerable
            .Range(0, amountOfCreatingBooking)
            .Select(_ => bookingRepo.TryCreateBookingAsync(@event.Id, _ct));

        // Act
        var result = await Task.WhenAll(tasks);
        var successfulBookings = result.Where(r => r.IsSuccess).ToList();
        var failedBookings = result.Where(r => !r.IsSuccess).ToList();

        // Assert
        Assert.Equal(expectedAvailableSeats, @event.AvailableSeats);
        Assert.Equal(availableSeats, successfulBookings.Count);
        Assert.Equal(amountOfCreatingBooking - availableSeats, failedBookings.Count);
        Assert.All(successfulBookings, sb =>
            {
                Assert.NotNull(sb.Booking);
                Assert.Equal(BookingStatus.Pending, sb.Booking.Status);
                Assert.Equal(@event.Id, sb.Booking.EventId);
            });
        Assert.All(failedBookings, fb =>
            {
                Assert.Null(fb.Booking);
                Assert.Equal(BookingErrorType.NoAvailableSeats, fb.ErrorType);
                Assert.Contains("No available seats", fb.ErrorMessage);
            });
    }

    [Fact]
    public async Task TryCreateBookingAsync_WithNoAvailableSeats_ShouldReturnFailure()
    {
        // Arrange
        Guid eventId = Guid.NewGuid();
        Event @event = EventFactory.Generate(
            eventId, "Title", DateTime.Now.AddDays(-1), DateTime.Now, 1, 0);

        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository.Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);
        var bookingRepo = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act
        var result = await bookingRepo.TryCreateBookingAsync(@event.Id, _ct);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Booking);
        Assert.Contains("No available seats for this event", result.ErrorMessage);
    }

    [Fact]
    public async Task TryCreateBookingAsync_WithNonExistentEventId_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentEventId = Guid.NewGuid();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(nonExistentEventId, _ct))
            .ReturnsAsync((Event?)null);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act
        var result = await repository.TryCreateBookingAsync(nonExistentEventId, _ct);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Booking);
        Assert.Contains($"Event with id '{nonExistentEventId}'", result.ErrorMessage);
    }

    [Fact]
    public async Task TryCreateBookingAsync_WhenCancellationRequested_ShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var mockEventRepository = new Mock<IEventRepository>();
        var repository = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            repository.TryCreateBookingAsync(Guid.NewGuid(), cts.Token));

        // Assert
        Assert.IsType<OperationCanceledException>(exception);
        Assert.Equal("The operation was canceled.", exception.Message);
    }

    #endregion

    #region TryGetBookingByIdAsync

    [Fact]
    public async Task TryGetBookingByIdAsync_WithExistingBooking_ShouldReturnBooking()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);

        // Act
        var getResult = await repository.TryGetBookingByIdAsync(
            createResult.Booking!.Id, _ct);

        // Assert
        Assert.True(getResult.IsSuccess);
        Assert.NotNull(getResult.Booking);
        Assert.Equal(createResult.Booking.Id, getResult.Booking.Id);
        Assert.Equal(BookingStatus.Pending, getResult.Booking.Status);
        Assert.Null(getResult.ErrorMessage);
    }

    [Fact]
    public async Task TryGetBookingByIdAsync_WithNonExistentBookingId_ShouldReturnFailure()
    {
        // Arrange
        var mockEventRepository = new Mock<IEventRepository>();
        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var nonExistentBookingId = Guid.NewGuid();

        // Act
        var result = await repository.TryGetBookingByIdAsync(nonExistentBookingId, _ct);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Booking);
        Assert.Contains($"Booking with id '{nonExistentBookingId}'", result.ErrorMessage);
    }

    [Fact]
    public async Task TryGetBookingByIdAsync_WhenEventWasDeletedAfterBookingCreation_ShouldRejectBooking()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);

        // Simulate event deletion
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync((Event?)null);

        // Act
        var getResult = await repository.TryGetBookingByIdAsync(
            createResult.Booking!.Id, _ct);

        // Assert
        Assert.False(getResult.IsSuccess);
        Assert.NotNull(getResult.Booking);
        Assert.Equal(BookingStatus.Rejected, getResult.Booking.Status);
        Assert.Contains($"Event with id '{@event.Id}'", getResult.ErrorMessage);
        Assert.NotNull(getResult.Booking.ProcessedAt);
    }

    [Fact]
    public async Task TryGetBookingByIdAsync_WhenCancellationRequested_ShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var mockEventRepository = new Mock<IEventRepository>();
        var repository = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            repository.TryGetBookingByIdAsync(Guid.NewGuid(), cts.Token));

        // Assert
        Assert.IsType<OperationCanceledException>(exception);
        Assert.Equal("The operation was canceled.", exception.Message);
    }

    #endregion

    #region TryGetPendingBooking

    [Fact]
    public async Task TryGetPendingBooking_WhenPendingBookingExists_ShouldReturnFirstPendingBooking()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createdBookingResult = await repository.TryCreateBookingAsync(@event.Id, _ct);

        // Act
        var pendingIds = (await repository.TryGetPendingBookingIds(_ct)).ToList();

        // Assert
        Assert.NotEmpty(pendingIds);
        Assert.Single(pendingIds);
        Assert.NotNull(createdBookingResult.Booking);
        Assert.Equal(createdBookingResult.Booking.Id, pendingIds[0]);
        Assert.Equal(BookingStatus.Pending, createdBookingResult.Booking.Status);
        Assert.Equal(@event.Id, createdBookingResult.Booking.EventId);
    }

    [Fact]
    public async Task TryGetPendingBooking_WhenNoPendingBookingsExist_ShouldReturnFailure()
    {
        // Arrange
        var mockEventRepository = new Mock<IEventRepository>();
        var repository = new BookingInMemoryRepository(mockEventRepository.Object);

        // Repository is empty, no bookings at all

        // Act
        var result = (await repository.TryGetPendingBookingIds(_ct)).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task TryGetPendingBooking_WhenAllBookingsAreConfirmedOrRejected_ShouldReturnFailure()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);

        // Create and confirm a booking
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        await repository.TryConfirmBooking(createResult.Booking!.Id, _ct);

        // Create and reject a booking
        var createResult2 = await repository.TryCreateBookingAsync(@event.Id, _ct);
        await repository.TryRejectBooking(createResult2.Booking!.Id, _ct);

        // Act
        var result = (await repository.TryGetPendingBookingIds(_ct)).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task TryGetPendingBooking_WhenCancellationRequested_ShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var mockEventRepository = new Mock<IEventRepository>();
        var repository = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            repository.TryGetPendingBookingIds(cts.Token));

        // Assert
        Assert.IsType<OperationCanceledException>(exception);
        Assert.Equal("The operation was canceled.", exception.Message);
    }

    #endregion

    #region TryConfirmBooking

    [Fact]
    public async Task TryConfirmBooking_WithValidPendingBooking_ShouldConfirmSuccessfully()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        var bookingId = createResult.Booking!.Id;

        // Act
        var confirmResult = await repository.TryConfirmBooking(bookingId, _ct);

        // Assert
        Assert.True(confirmResult.IsSuccess);
        Assert.NotNull(confirmResult.Booking);
        Assert.Equal(BookingStatus.Confirmed, confirmResult.Booking.Status);
        Assert.NotNull(confirmResult.Booking.ProcessedAt);

        // Verify booking is actually updated in repository
        var getResult = await repository.TryGetBookingByIdAsync(bookingId, _ct);
        Assert.Equal(BookingStatus.Confirmed, getResult.Booking!.Status);
    }

    [Fact]
    public async Task TryConfirmBooking_WithNonExistentBooking_ShouldReturnFailure()
    {
        // Arrange
        var mockEventRepository = new Mock<IEventRepository>();
        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var nonExistentBookingId = Guid.NewGuid();

        // Act
        var result = await repository.TryConfirmBooking(nonExistentBookingId, _ct);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Booking);
        Assert.Contains($"Booking with id '{nonExistentBookingId}'", result.ErrorMessage);
    }

    [Fact]
    public async Task TryConfirmBooking_WhenEventWasDeletedAfterBookingCreation_ShouldRejectBooking()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);

        // Simulate event deletion
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync((Event?)null);

        // Act
        var confirmResult = await repository.TryConfirmBooking(createResult.Booking!.Id, _ct);

        // Assert
        Assert.False(confirmResult.IsSuccess);
        Assert.NotNull(confirmResult.Booking);
        Assert.Equal(BookingStatus.Rejected, confirmResult.Booking.Status);
        Assert.Contains($"Event with id '{@event.Id}'", confirmResult.ErrorMessage);
        Assert.NotNull(confirmResult.Booking.ProcessedAt);
    }

    [Fact]
    public async Task TryConfirmBooking_WhenBookingAlreadyConfirmed_ShouldReturnSuccessWithoutChanges()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        var bookingId = createResult.Booking!.Id;

        // Confirm once
        await repository.TryConfirmBooking(bookingId, _ct);
        var firstConfirmResult = await repository.TryGetBookingByIdAsync(bookingId, _ct);
        var firstProcessedAt = firstConfirmResult.Booking!.ProcessedAt;

        // Act - confirm again
        var secondConfirmResult = await repository.TryConfirmBooking(bookingId, _ct);

        // Assert
        Assert.True(secondConfirmResult.IsSuccess);
        Assert.NotNull(secondConfirmResult.Booking);
        Assert.Equal(BookingStatus.Confirmed, secondConfirmResult.Booking.Status);
        // ProcessedAt should remain the same (not updated again)
        Assert.Equal(firstProcessedAt, secondConfirmResult.Booking.ProcessedAt);
    }

    [Fact]
    public async Task TryConfirmBooking_WhenBookingAlreadyRejected_ShouldReturnFailure()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        var bookingId = createResult.Booking!.Id;

        // Reject the booking
        await repository.TryRejectBooking(bookingId, _ct);

        // Act
        var confirmResult = await repository.TryConfirmBooking(bookingId, _ct);

        // Assert
        Assert.False(confirmResult.IsSuccess);
        Assert.NotNull(confirmResult.Booking);
        Assert.Equal(BookingStatus.Rejected, confirmResult.Booking.Status);
        Assert.Contains($"not in {BookingStatus.Pending} status", confirmResult.ErrorMessage);
    }

    [Fact]
    public async Task TryConfirmBooking_WhenCancellationRequested_ShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var mockEventRepository = new Mock<IEventRepository>();
        var repository = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            repository.TryConfirmBooking(Guid.NewGuid(), cts.Token));

        // Assert
        Assert.IsType<OperationCanceledException>(exception);
        Assert.Equal("The operation was canceled.", exception.Message);
    }

    #endregion

    #region TryRejectBooking

    [Fact]
    public async Task TryRejectBooking_WithValidPendingBooking_ShouldRejectSuccessfully()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        var bookingId = createResult.Booking!.Id;

        // Act
        var rejectResult = await repository.TryRejectBooking(bookingId, _ct);

        // Assert
        Assert.True(rejectResult.IsSuccess);
        Assert.NotNull(rejectResult.Booking);
        Assert.Equal(BookingStatus.Rejected, rejectResult.Booking.Status);
        Assert.NotNull(rejectResult.Booking.ProcessedAt);

        // Verify booking is actually updated in repository
        var getResult = await repository.TryGetBookingByIdAsync(bookingId, _ct);
        Assert.Equal(BookingStatus.Rejected, getResult.Booking!.Status);
    }

    [Fact]
    public async Task TryRejectBooking_WithValidPendingBooking_ShouldReleaseAvailableEventSeat()
    {
        // Arrange
        Guid eventId = Guid.NewGuid();
        int baseAvailableSeats = 1;
        Event @event = EventFactory.Generate(
            eventId,
            "Title",
            DateTime.Now.AddDays(-1),
            DateTime.Now,
            baseAvailableSeats,
            baseAvailableSeats);
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        var bookingId = createResult.Booking!.Id;
        var availableSeatsAfterReserving = @event.AvailableSeats;

        // Act
        var rejectResult = await repository.TryRejectBooking(bookingId, _ct);

        // Assert
        Assert.True(rejectResult.IsSuccess);
        Assert.NotNull(rejectResult.Booking);
        Assert.Equal(BookingStatus.Rejected, rejectResult.Booking.Status);
        Assert.True(availableSeatsAfterReserving == baseAvailableSeats - 1);
        Assert.True(baseAvailableSeats == @event.AvailableSeats);
    }

    [Fact]
    public async Task TryRejectBooking_WithNonExistentBooking_ShouldReturnFailure()
    {
        // Arrange
        var mockEventRepository = new Mock<IEventRepository>();
        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var nonExistentBookingId = Guid.NewGuid();

        // Act
        var result = await repository.TryRejectBooking(nonExistentBookingId, _ct);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Booking);
        Assert.Contains($"Booking with id '{nonExistentBookingId}'", result.ErrorMessage);
    }

    [Fact]
    public async Task TryRejectBooking_WhenEventWasDeleted_ShouldStillRejectSuccessfully()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        var bookingId = createResult.Booking!.Id;

        // Simulate event deletion (should not affect rejection)
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync((Event?)null);

        // Act
        var rejectResult = await repository.TryRejectBooking(bookingId, _ct);

        // Assert
        Assert.True(rejectResult.IsSuccess);
        Assert.NotNull(rejectResult.Booking);
        Assert.Equal(BookingStatus.Rejected, rejectResult.Booking.Status);
    }

    [Fact]
    public async Task TryRejectBooking_WhenBookingAlreadyRejected_ShouldReturnSuccessWithoutChanges()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        var bookingId = createResult.Booking!.Id;

        // Reject once
        await repository.TryRejectBooking(bookingId, _ct);
        var firstRejectResult = await repository.TryGetBookingByIdAsync(bookingId, _ct);
        var firstProcessedAt = firstRejectResult.Booking!.ProcessedAt;

        // Act - reject again
        var secondRejectResult = await repository.TryRejectBooking(bookingId, _ct);

        // Assert
        Assert.True(secondRejectResult.IsSuccess);
        Assert.NotNull(secondRejectResult.Booking);
        Assert.Equal(BookingStatus.Rejected, secondRejectResult.Booking.Status);
        // ProcessedAt should remain the same
        Assert.Equal(firstProcessedAt, secondRejectResult.Booking.ProcessedAt);
    }

    [Fact]
    public async Task TryRejectBooking_WhenBookingAlreadyConfirmed_ShouldRejectIt()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        var bookingId = createResult.Booking!.Id;

        // Confirm first
        await repository.TryConfirmBooking(bookingId, _ct);

        // Act - reject after confirmation
        var rejectResult = await repository.TryRejectBooking(bookingId, _ct);

        // Assert
        Assert.True(rejectResult.IsSuccess);
        Assert.NotNull(rejectResult.Booking);
        Assert.Equal(BookingStatus.Rejected, rejectResult.Booking.Status);
    }

    [Fact]
    public async Task TryRejectBooking_WhenCancellationRequested_ShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var mockEventRepository = new Mock<IEventRepository>();
        var repository = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            repository.TryRejectBooking(Guid.NewGuid(), cts.Token));

        // Assert
        Assert.IsType<OperationCanceledException>(exception);
        Assert.Equal("The operation was canceled.", exception.Message);
    }

    #endregion

    #region Integration/Combined Scenarios

    [Fact]
    public async Task CompleteBookingLifecycle_CreateConfirmGet_ShouldWorkCorrectly()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act - Create
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        var bookingId = createResult.Booking!.Id;

        // Act - Confirm
        var confirmResult = await repository.TryConfirmBooking(bookingId, _ct);

        // Act - Get
        var getResult = await repository.TryGetBookingByIdAsync(bookingId, _ct);

        // Assert
        Assert.True(createResult.IsSuccess);
        Assert.True(confirmResult.IsSuccess);
        Assert.True(getResult.IsSuccess);
        Assert.Equal(BookingStatus.Confirmed, getResult.Booking!.Status);
    }

    [Fact]
    public async Task CompleteBookingLifecycle_CreateRejectGet_ShouldWorkCorrectly()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);

        // Act - Create
        var createResult = await repository.TryCreateBookingAsync(@event.Id, _ct);
        var bookingId = createResult.Booking!.Id;

        // Act - Reject
        var rejectResult = await repository.TryRejectBooking(bookingId, _ct);

        // Act - Get
        var getResult = await repository.TryGetBookingByIdAsync(bookingId, _ct);

        // Assert
        Assert.True(createResult.IsSuccess);
        Assert.True(rejectResult.IsSuccess);
        Assert.True(getResult.IsSuccess);
        Assert.Equal(BookingStatus.Rejected, getResult.Booking!.Status);
    }

    [Fact]
    public async Task MultipleBookingsForSameEvent_ShouldAllBeCreatedIndependently()
    {
        // Arrange
        Event @event = EventFactory.Generate();
        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.TryGetByIdAsync(@event.Id, _ct))
            .ReturnsAsync(@event);

        var repository = new BookingInMemoryRepository(mockEventRepository.Object);
        const int bookingCount = 5;

        // Act
        var createTasks = Enumerable.Range(0, bookingCount)
            .Select(_ => repository.TryCreateBookingAsync(@event.Id, _ct))
            .ToArray();

        var results = await Task.WhenAll(createTasks);

        // Assert
        Assert.All(results, r => Assert.True(r.IsSuccess));
        Assert.All(results, r => Assert.Equal(@event.Id, r.Booking!.EventId));
        Assert.All(results, r => Assert.Equal(BookingStatus.Pending, r.Booking!.Status));

        var uniqueIds = results.Select(r => r.Booking!.Id).Distinct().Count();
        Assert.Equal(bookingCount, uniqueIds);
    }

    #endregion
}
