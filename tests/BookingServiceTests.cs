using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
using BookingTests.Helpers;
using Moq;

namespace BookingTests;

public class BookingServiceTests
{
    private readonly CancellationToken _ct = CancellationToken.None;

    [Fact]
    public async Task CreateBookingAsync_ValidEventId_ReturnsCreatedBooking()
    {
        throw new NotImplementedException();
        // // Arrange
        // Event @event = EventFactory.Generate();
        // var booking = BookingFactory.Generate(@event.Id);

        // var mockBookingRepository = new Mock<IBookingRepository>();
        // mockBookingRepository
        //     .Setup(r => r.TryCreateBookingAsync(@event.Id, _ct))
        //     .ReturnsAsync(BookingRepositoryResult.Success(booking));

        // var bookingService = new BookingService(mockBookingRepository.Object);

        // // Act
        // var res = await bookingService.CreateBookingAsync(@event.Id, _ct);

        // // Assert
        // Assert.NotNull(res);
        // Assert.True(booking.IsEqual(res));
    }

    [Fact]
    public async Task CreateBookingAsync_ConcurrentCallsWithSameValidEventId_ReturnsCreatedBookings()
    {
        throw new NotImplementedException();
        // // Arrange
        // Event @event = EventFactory.Generate();

        // var mockBookingRepository = new Mock<IBookingRepository>();
        // mockBookingRepository
        //     .Setup(r => r.TryCreateBookingAsync(@event.Id, _ct))
        //     .ReturnsAsync(() => BookingRepositoryResult
        //         .Success(BookingFactory.Generate(@event.Id)));

        // var bookingService = new BookingService(mockBookingRepository.Object);

        // // Act
        // var res = await Task.WhenAll(
        //     bookingService.CreateBookingAsync(@event.Id, _ct),
        //     bookingService.CreateBookingAsync(@event.Id, _ct));

        // // Assert
        // Assert.True(res[0].Id != res[1].Id);
    }

    [Fact]
    public async Task GetBookingByIdAsync_ValidBookingId_ReturnsCorrectBooking()
    {
        throw new NotImplementedException();
        // // Arrange
        // var eventId = Guid.NewGuid();
        // var booking = BookingFactory.Generate(eventId);

        // var mockBookingRepository = new Mock<IBookingRepository>();
        // mockBookingRepository
        //     .Setup(r => r.TryGetBookingByIdAsync(booking.Id, _ct))
        //     .ReturnsAsync(BookingRepositoryResult.Success(booking));

        // var bookingService = new BookingService(mockBookingRepository.Object);

        // // Act
        // var res = await bookingService.GetBookingByIdAsync(booking.Id, _ct);

        // // Assert
        // Assert.NotNull(res);
        // Assert.True(booking.IsEqual(res));
    }

    [Fact]
    public async Task CreateBookingAsync_ValidBookingId_ThrowsEventNotFoundException()
    {
        throw new NotImplementedException();
        // // Arrange
        // Guid eventId = Guid.NewGuid();
        // var booking = BookingFactory.Generate(eventId);

        // var mockBookingRepository = new Mock<IBookingRepository>();
        // mockBookingRepository
        //     .Setup(r => r.TryCreateBookingAsync(It.Is<Guid>(id => id == eventId), _ct))
        //     .ReturnsAsync(BookingRepositoryResult.EventNotFound(eventId));

        // var bookingService = new BookingService(mockBookingRepository.Object);

        // // Act
        // var exception = await Record
        //     .ExceptionAsync(() => bookingService.CreateBookingAsync(eventId, _ct));

        // // Assert
        // Assert.IsType<EventNotFoundException>(exception);
        // Assert.Equal($"Event with Id '{eventId}' was not found.", exception.Message);
    }

    [Fact]
    public async Task CreateBookingAsync_NonExistentEventId_ThrowsNoAvailableSeatsException()
    {
        throw new NotImplementedException();
        // // Arrange
        // var eventId = Guid.NewGuid();
        // var booking = BookingFactory.Generate(eventId);

        // var mockBookingRepository = new Mock<IBookingRepository>();
        // mockBookingRepository
        //     .Setup(r => r.TryCreateBookingAsync(eventId, _ct))
        //     .ReturnsAsync(BookingRepositoryResult.NoAvailableSeats());

        // var bookingService = new BookingService(mockBookingRepository.Object);

        // // Act
        // var exception = await Record
        //     .ExceptionAsync(() => bookingService.CreateBookingAsync(eventId, _ct));

        // // Assert
        // Assert.IsType<NoAvailableSeatsException>(exception);
        // Assert.Equal("No available seats for this event", exception.Message);
    }

    [Fact]
    public async Task GetBookingByIdAsync_NonExistentBookingId_ThrowsBookingNotFoundException()
    {
        throw new NotImplementedException();
        // // Arrange
        // Guid bookingId = Guid.NewGuid();

        // var mockBookingRepository = new Mock<IBookingRepository>();
        // mockBookingRepository
        //     .Setup(r => r.TryGetBookingByIdAsync(It.Is<Guid>(id => id == bookingId), _ct))
        //     .ReturnsAsync(BookingRepositoryResult.BookingNotFound(bookingId));

        // var bookingService = new BookingService(mockBookingRepository.Object);

        // // Act
        // var exception = await Record
        //     .ExceptionAsync(() => bookingService.GetBookingByIdAsync(bookingId, _ct));

        // // Assert
        // Assert.IsType<BookingNotFoundException>(exception);
        // Assert.Equal($"Booking with Id '{bookingId}' was not found.", exception.Message);
    }
}
