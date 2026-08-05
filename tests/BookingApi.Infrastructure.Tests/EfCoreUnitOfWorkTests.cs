using BookingApi.Application.Interfaces;
using BookingApi.Infrastructure.Repositories;
using BookingApi.Infrastructure.Tests.Base;
using BookingApi.Infrastructure.Tests.Helpers;
using BookingApi.Infrastructure.UnitOfWork;
using System.Data;

namespace BookingApi.Infrastructure.Tests;

public class EfCoreUnitOfWorkTests : PostgreSqlBase
{
    [Fact]
    public async Task BeginTransactionAndCommit_ShouldCommitAllChanges()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var userRepository = new EFCoreUserRepository(context);
        var user = UserFactory.Generate();
        userRepository.Add(user);
        var unitOfWork = new EfCoreUnitOfWork(
            context, eventRepository, bookingRepository, userRepository);

        var @event = EventFactory.Generate();
        var booking = BookingFactory.Generate(@event.Id, user.Id);

        // Act
        await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        unitOfWork.EventRepository.Add(@event);
        unitOfWork.BookingRepository.Add(booking);

        await unitOfWork.SaveChangesAsync();
        await unitOfWork.CommitTransactionAsync();
        context.ChangeTracker.Clear();

        // Assert
        var createdEvent = await eventRepository
            .FirstOrDefaultAsync(e => e.Id == @event.Id);
        var createdBooking = await bookingRepository
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        Assert.NotNull(createdEvent);
        Assert.NotNull(createdBooking);
        Assert.Equal(@event.Id, createdBooking.EventId);
    }

    [Fact]
    public async Task TransactionRollback_ShouldNotSaveAnyChanges()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var userRepository = new EFCoreUserRepository(context);

        var unitOfWork = new EfCoreUnitOfWork(
            context, eventRepository, bookingRepository, userRepository);

        var @event = EventFactory.Generate();
        var user = UserFactory.Generate();
        userRepository.Add(user);
        var booking = BookingFactory.Generate(@event.Id, user.Id);

        // Act
        await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        unitOfWork.EventRepository.Add(@event);
        unitOfWork.BookingRepository.Add(booking);
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.RollbackTransactionAsync();
        context.ChangeTracker.Clear();

        var createdEvent = await eventRepository
            .FirstOrDefaultAsync(e => e.Id == @event.Id);
        var createdBooking = await bookingRepository
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        // Assert
        Assert.Null(createdEvent);
        Assert.Null(createdBooking);
    }

    [Fact]
    public async Task SaveChangesAsync_WithoutTransaction_ShouldSaveChanges()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var userRepository = new EFCoreUserRepository(context);

        var unitOfWork = new EfCoreUnitOfWork(
            context, eventRepository, bookingRepository, userRepository);

        var user = UserFactory.Generate();
        userRepository.Add(user);
        var @event = EventFactory.Generate();
        var booking = BookingFactory.Generate(@event.Id, user.Id);

        // Act
        unitOfWork.EventRepository.Add(@event);
        unitOfWork.BookingRepository.Add(booking);
        var savedCount = await unitOfWork.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var createdEvent = await eventRepository
            .FirstOrDefaultAsync(e => e.Id == @event.Id);
        var createdBooking = await bookingRepository
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        // Assert
        Assert.Equal(3, savedCount);
        Assert.NotNull(createdEvent);
        Assert.NotNull(createdBooking);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_ShouldRetryOnConcurrencyException()
    {
        // Arrange
        var context1 = CreateContext();
        var context2 = CreateContext();

        var eventRepository1 = new EFCoreEventRepository(context1);
        var bookingRepository1 = new EFCoreBookingRepository(context1);
        var userRepository1 = new EFCoreUserRepository(context1);
        var unitOfWork1 = new EfCoreUnitOfWork(
            context1, eventRepository1, bookingRepository1, userRepository1);
        var eventRepository2 = new EFCoreEventRepository(context2);

        var @event = EventFactory.Generate();
        unitOfWork1.EventRepository.Add(@event);
        await unitOfWork1.SaveChangesAsync();

        // Act
        var result = await unitOfWork1.ExecuteWithRetryAsync(async ct =>
        {
            var event1 = await eventRepository1
                .FirstOrDefaultAsync(e => e.Id == @event.Id, ct);

            var event2 = await eventRepository2
                .FirstOrDefaultAsync(e => e.Id == @event.Id, ct);
            event2!.Title = "Updated by other";
            await context2.SaveChangesAsync(ct);

            event1!.Title = "Updated by first";
            await unitOfWork1.SaveChangesAsync(ct);

            return true;
        }, CancellationToken.None, maxRetries: 3);

        var finalEvent = await eventRepository1
            .FirstOrDefaultAsync(e => e.Id == @event.Id);

        // Assert
        Assert.True(result);
        Assert.Equal("Updated by first", finalEvent!.Title);
    }

    [Fact]
    public async Task Dispose_ShouldDisposeContextAndTransaction()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var userRepository = new EFCoreUserRepository(context);
        var unitOfWork = new EfCoreUnitOfWork(
            context, eventRepository, bookingRepository, userRepository);

        // Act
        await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        unitOfWork.Dispose();

        var exception = await Record.ExceptionAsync(() =>
            unitOfWork.EventRepository.FirstOrDefaultAsync(e => e.Id == Guid.NewGuid()));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ObjectDisposedException>(exception);
    }

    [Fact]
    public async Task MultipleOperationsInTransaction_ShouldAllSucceed()
    {
        // Arrange
        var context = CreateContext();
        var eventRepository = new EFCoreEventRepository(context);
        var bookingRepository = new EFCoreBookingRepository(context);
        var userRepository = new EFCoreUserRepository(context);
        var unitOfWork = new EfCoreUnitOfWork(
            context, eventRepository, bookingRepository, userRepository);

        var user = UserFactory.Generate();
        userRepository.Add(user);
        var @event1 = EventFactory.Generate();
        var @event2 = EventFactory.Generate();
        var booking1 = BookingFactory.Generate(@event1.Id, user.Id);
        var booking2 = BookingFactory.Generate(@event2.Id, user.Id);

        // Act
        await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        unitOfWork.EventRepository.Add(@event1);
        unitOfWork.EventRepository.Add(@event2);
        unitOfWork.BookingRepository.Add(booking1);
        unitOfWork.BookingRepository.Add(booking2);

        await unitOfWork.SaveChangesAsync();
        await unitOfWork.CommitTransactionAsync();

        // Assert
        var createdEvents = await eventRepository.ToListAsync(
            eventRepository.GetQuery(QueryTrackerBehavior.NoTracking));
        var createdBookings = await bookingRepository.ToListAsync(
            bookingRepository.GetQuery(QueryTrackerBehavior.NoTracking));

        Assert.Equal(2, createdEvents.Count);
        Assert.Equal(2, createdBookings.Count);
    }
}
