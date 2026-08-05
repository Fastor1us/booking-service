using BookingApi.Application.Interfaces;
using BookingApi.Domain.Constants;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;

namespace BookingApi.Application.Services;

public class BookingService(IUnitOfWork unitOfWork) : IBookingService
{
    public async Task<Booking> AddAsync(
        Guid eventId,
        string userLogin,
        CancellationToken ct)
    {
        var res = await unitOfWork.ExecuteWithRetryAsync(async _ =>
            {
                var user = await unitOfWork.UserReopitory
                    .FirstOrDefaultAsync(QueryTrackerBehavior.NoTracking,
                        e => e.Login == userLogin, ct)
                    ?? throw new UserNotFoundException(userLogin);

                var bookingQuery = unitOfWork.BookingRepository
                    .GetQuery(QueryTrackerBehavior.NoTracking)
                    .Where(e => e.UserId == user.Id 
                        && (e.Status == BookingStatus.Pending 
                            || e.Status == BookingStatus.Confirmed));
                var userBookingCount = await unitOfWork.BookingRepository
                    .CountAsync(bookingQuery, ct);

                if (userBookingCount >= UserConstant.MaxActiveBookings)
                {
                    throw new BookingExceedLimitException(userLogin);
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
                    UserId = user.Id,
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

    public async Task CancelAsync(
        Guid bookingId,
        string userLogin,
        CancellationToken ct)
    {
        var booking = await unitOfWork.BookingRepository
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new BookingNotFoundException(bookingId);

        var user = await unitOfWork.UserReopitory
            .FirstOrDefaultAsync(b => b.Login == userLogin, ct)
            ?? throw new UserNotFoundException(userLogin);

        if (user.Role != UserRole.Admin
            && booking.UserId != user.Id)
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
        }
    }
}
