using BookingApi.Application.Interfaces;
using BookingApi.Domain.Constants;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Services;

public class BookingService(IUnitOfWork unitOfWork) : IBookingService
{
    public async Task<Booking> AddAsync(
        Guid eventId,
        Guid userId,
        CancellationToken ct)
    {
        var res = await unitOfWork.ExecuteWithRetryAsync(async _ =>
            {
                var bookingQuery = unitOfWork.BookingRepository
                    .GetQuery(QueryTrackerBehavior.NoTrackingWithIdentityResolution);
                var usersBookings = await unitOfWork.BookingRepository
                    .CountAsync(bookingQuery.Where(
                        e => e.UserId == userId 
                        && e.Status == BookingStatus.Confirmed), ct);

                if (usersBookings >= UserConstant.MaxActiveBookings)
                {
                    throw new BookingExceedLimitException(userId);
                }

                var @event = await unitOfWork.EventRepository
                    .FirstOrDefaultAsync(e => e.Id == eventId, ct)
                    ?? throw new EventNotFoundException(eventId);

                if (@event.StartAt <= DateTime.Now)
                {
                    throw new BookingPastEventException(@event.Id);
                }

                if (@event.AvailableSeats == 0)
                    throw new NoAvailableSeatsException(eventId);

                @event.AvailableSeats--;

                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    UserId = userId,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                unitOfWork.BookingRepository.Add(booking);
                await unitOfWork.SaveChangesAsync(ct);

                return booking;
            },
            ct);

        return res;
    }

    public async Task<Booking> GetByIdAsync(
        Guid bookingId,
        CancellationToken ct)
    {
        return await unitOfWork.BookingRepository
            .FirstOrDefaultAsync(
                QueryTrackerBehavior.NoTracking,
                e => e.Id == bookingId,
                ct)
            ?? throw new BookingNotFoundException(bookingId);
    }

    public async Task Cancel(
        Guid bookingId,
        Guid userId,
        CancellationToken ct)
    {
        await unitOfWork.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted, ct);

        var booking = await unitOfWork.BookingRepository
                .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
                ?? throw new BookingNotFoundException(bookingId);

        if (booking.User.Role != UserRole.Admin
            && booking.UserId != userId)
        {
            throw new ForbiddenException();
        }

        if (booking.Status != BookingStatus.Cancelled)
        {
            if (booking.Status == BookingStatus.Confirmed
                || booking.Status == BookingStatus.Pending)
            {
                booking.Event?.AvailableSeats += 1;
            }

            booking.Status = BookingStatus.Cancelled;

            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
        }
        else
        {
            await unitOfWork.RollbackTransactionAsync(ct);
        }
    }
}
