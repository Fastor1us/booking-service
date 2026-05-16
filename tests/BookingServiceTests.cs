using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
using BookingTests.Helpers;
using Moq;

namespace BookingTests;

public class BookingServiceTests
{
    private readonly CancellationToken _ct = new CancellationTokenSource().Token;

    [Fact]
    public async Task CreateBooking_ValidEventId_ReturnsCreatedBooking()
    {
        // Arrange
        Event @event = EventFactory.CreateEvent();

        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == @event.Id), _ct))
            .ReturnsAsync(@event);

        var booking = BookingFactory.CreateBooking(@event.Id);

        var mockBookingRepository = new Mock<IBookingRepository>();
        mockBookingRepository
            .Setup(r => r.CreateBookingAsync(@event.Id, _ct))
            .ReturnsAsync(booking);

        var bookingService = new BookingService(
            mockEventRepository.Object, mockBookingRepository.Object);

        // Act
        var res = await bookingService.CreateBookingAsync(@event.Id, _ct);

        // Assert
        Assert.NotNull(res);
        Assert.True(booking.IsEqual(res));
    }

    [Fact]
    public async Task CreateBooking_SeveralAtOnceWithSameEventId_ReturnsCreatedBookings()
    {
        // Arrange
        Event @event = EventFactory.CreateEvent();

        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == @event.Id), _ct))
            .ReturnsAsync(@event);

        var mockBookingRepository = new Mock<IBookingRepository>();
        mockBookingRepository
            .Setup(r => r.CreateBookingAsync(@event.Id, _ct))
            .ReturnsAsync(BookingFactory.CreateBooking(@event.Id));

        var bookingService = new BookingService(
            mockEventRepository.Object, mockBookingRepository.Object);

        // Act
        var res1 = bookingService.CreateBookingAsync(@event.Id, _ct);
        var res2 = bookingService.CreateBookingAsync(@event.Id, _ct);

        await Task.WhenAll(res1, res2);

        // Assert
        Assert.True(res1.Id != res2.Id);
    }

    [Fact]
    public async Task GetBookingById_ValidBookingId_ReturnsCorrectBooking()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        var mockEventRepository = new Mock<IEventRepository>();

        var booking = BookingFactory.CreateBooking(eventId);

        var mockBookingRepository = new Mock<IBookingRepository>();
        mockBookingRepository
            .Setup(r => r.GetBookingByIdAsync(booking.Id, _ct))
            .ReturnsAsync(booking);

        var bookingService = new BookingService(
            mockEventRepository.Object, mockBookingRepository.Object);

        // Act
        var res = await bookingService.GetBookingByIdAsync(booking.Id, _ct);

        // Assert
        Assert.NotNull(res);
        Assert.True(booking.IsEqual(res));
    }

    [Fact]
    public async Task CreateBooking_NonExistentEventId_EventNotFoundException()
    {
        // Arrange
        Guid eventId = Guid.NewGuid();

        var mockEventRepository = new Mock<IEventRepository>();
        mockEventRepository
            .Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == eventId), _ct))
            .Throws(new EventNotFoundException(eventId));

        var booking = BookingFactory.CreateBooking(eventId);

        var mockBookingRepository = new Mock<IBookingRepository>();

        var bookingService = new BookingService(
            mockEventRepository.Object, mockBookingRepository.Object);

        // Act
        var exception = await Record
            .ExceptionAsync(() => bookingService.CreateBookingAsync(eventId, _ct));

        // Assert
        Assert.IsType<EventNotFoundException>(exception);
        Assert.Equal(exception.Message, $"Event with Id '{eventId}' was not found.");
    }

    [Fact]
    public async Task GetBookingById_NonExistentBookingId_ThrowsBookingNotFoundException()
    {
        // Arrange
        Guid bookingId = Guid.NewGuid();

        var mockEventRepository = new Mock<IEventRepository>();

        var mockBookingRepository = new Mock<IBookingRepository>();
        mockBookingRepository
            .Setup(r => r.GetBookingByIdAsync(It.Is<Guid>(id => id == bookingId), _ct))
            .Throws(new BookingNotFoundException(bookingId));

        var bookingService = new BookingService(
            mockEventRepository.Object, mockBookingRepository.Object);

        // Act
        var exception = await Record
            .ExceptionAsync(() => bookingService.GetBookingByIdAsync(bookingId, _ct));

        // Assert
        Assert.IsType<BookingNotFoundException>(exception);
        Assert.Equal(exception.Message, $"Booking with Id '{bookingId}' was not found.");
    }
}
