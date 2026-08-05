using BookingApi.Application.Interfaces;
using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Repositories;
using BookingApi.Infrastructure.Tests.Base;
using BookingApi.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Infrastructure.Tests;

public class EFCoreBookingRepositoryTests : PostgreSqlBase
{
    #region Add
    [Fact]
    public async Task Add_Booking_ShouldAddSuccessfully()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var userRepository = new EFCoreUserRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);
        var user = UserFactory.Generate();
        userRepository.Add(user);
        var booking = BookingFactory.Generate(@event.Id, user.Id);
        bookingRepository.Add(booking);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var createdBooking = await bookingRepository
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        // Assert
        Assert.NotNull(createdBooking);
        Assert.Equal(booking.Id, createdBooking.Id);
    }

    [Fact]
    public async Task Add_BookingWithNonExistentEventId_ShouldThrowDbUpdateException()
    {
        // Arrange
        var context = CreateContext();
        var bookingRepository = new EFCoreBookingRepository(context);
        var userRepository = new EFCoreUserRepository(context);
        var user = UserFactory.Generate();
        userRepository.Add(user);
        var nonExistentEventId = Guid.NewGuid();
        var booking = BookingFactory.Generate(nonExistentEventId, user.Id);
        bookingRepository.Add(booking);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            context.SaveChangesAsync());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<DbUpdateException>(exception);
    }

    [Fact]
    public async Task Add_MultipleBookings_ShouldAddAll()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var userRepository = new EFCoreUserRepository(context);
        var user = UserFactory.Generate();
        userRepository.Add(user);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);
        var bookings = BookingFactory.Generate(@event.Id, user.Id, 3);
        foreach (var b in bookings)
        {
            bookingRepository.Add(b);
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var allBookings = await bookingRepository.ToListAsync(
            bookingRepository.GetQuery(QueryTrackerBehavior.NoTracking));

        // Assert
        Assert.Equal(3, allBookings.Count);
    }
    #endregion

    #region FirstOrDefaultAsync
    [Fact]
    public async Task FirstOrDefaultAsync_ValidBookingId_ReturnsCorrectBooking()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);
        var userRepository = new EFCoreUserRepository(context);
        var user = UserFactory.Generate();
        userRepository.Add(user);
        var booking = BookingFactory.Generate(@event.Id, user.Id);
        bookingRepository.Add(booking);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var createdBooking = await bookingRepository
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        // Assert
        Assert.NotNull(createdBooking);
        Assert.Equal(booking.Id, createdBooking.Id);
        Assert.Equal(booking.EventId, createdBooking.EventId);
        Assert.Equal(booking.Status, createdBooking.Status);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ByEventId_ShouldReturnBooking()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);
        var userRepository = new EFCoreUserRepository(context);
        var user = UserFactory.Generate();
        userRepository.Add(user);
        var booking = BookingFactory.Generate(@event.Id, user.Id);
        bookingRepository.Add(booking);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var foundBooking = await bookingRepository
            .FirstOrDefaultAsync(b => b.EventId == @event.Id);

        // Assert
        Assert.NotNull(foundBooking);
        Assert.Equal(booking.Id, foundBooking.Id);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_NonExistentBookingId_ReturnsNull()
    {
        // Arrange
        var context = CreateContext();
        var bookingRepository = new EFCoreBookingRepository(context);

        // Act
        var booking = await bookingRepository
            .FirstOrDefaultAsync(b => b.Id == Guid.NewGuid());

        // Assert
        Assert.Null(booking);
    }
    #endregion

    #region GetQuery
    [Fact]
    public async Task GetQuery_WithFilterByStatus_ShouldReturnCorrectBookings()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var userRepository = new EFCoreUserRepository(context);

        var user = UserFactory.Generate();
        userRepository.Add(user);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);
        await context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            BookingFactory.Generate(@event.Id, user.Id, bookingStatus: BookingStatus.Pending),
            BookingFactory.Generate(@event.Id, user.Id, bookingStatus: BookingStatus.Pending),
            BookingFactory.Generate(@event.Id, user.Id, bookingStatus: BookingStatus.Confirmed)
        };
        foreach (var b in bookings)
        {
            bookingRepository.Add(b);
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var pendingBookings = await bookingRepository.ToListAsync(
            bookingRepository
                .GetQuery(QueryTrackerBehavior.NoTracking)
                .Where(b => b.Status == BookingStatus.Pending));

        // Assert
        Assert.Equal(2, pendingBookings.Count);
        Assert.All(pendingBookings, b =>
            Assert.Equal(BookingStatus.Pending, b.Status));
    }
    #endregion

    #region Update
    [Fact]
    public async Task Update_BookingStatus_ShouldPersistChange()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var userRepository = new EFCoreUserRepository(context);
        var user = UserFactory.Generate();
        userRepository.Add(user);
        var @event = EventFactory.Generate();
        var booking = BookingFactory.Generate(@event.Id, user.Id);
        eventRepository.Add(@event);
        bookingRepository.Add(booking);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var retrievedBooking = await bookingRepository
            .FirstOrDefaultAsync(b => b.Id == booking.Id);
        retrievedBooking!.Status = BookingStatus.Confirmed;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var updatedBooking = await bookingRepository
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        // Assert
        Assert.Equal(BookingStatus.Confirmed, updatedBooking!.Status);
    }
    #endregion

    #region CountAsync
    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var userRepository = new EFCoreUserRepository(context);
        var user = UserFactory.Generate();
        userRepository.Add(user);
        var @event = EventFactory.Generate();
        eventRepository.Add(@event);
        var bookings = BookingFactory.Generate(@event.Id, user.Id, 5);
        foreach (var b in bookings)
        {
            bookingRepository.Add(b);
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var count = await bookingRepository.CountAsync(
            bookingRepository.GetQuery(QueryTrackerBehavior.NoTracking));

        // Assert
        Assert.Equal(5, count);
    }
    #endregion
}
