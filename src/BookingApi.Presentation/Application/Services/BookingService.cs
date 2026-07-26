using BookingApi.Presentation.Application.Interfaces;
using BookingApi.Presentation.Domain.Exceptions;
using BookingApi.Presentation.Domain.Models;

namespace BookingApi.Presentation.Application.Services;

public class BookingService(IUnitOfWork unitOfWork) : IBookingService
{
    public async Task<Booking> AddAsync(Guid eventId, CancellationToken ct)
    {
        var res = await unitOfWork.ExecuteWithRetryAsync(async _ =>
            {
                var @event = await unitOfWork.EventRepository
                    .FirstOrDefaultAsync(e => e.Id == eventId, ct)
                    ?? throw new EventNotFoundException(eventId);

                if (@event.AvailableSeats == 0)
                    throw new NoAvailableSeatsException(eventId);

                @event.AvailableSeats--;

                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
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

    public async Task<Booking> GetByIdAsync(Guid bookingId, CancellationToken ct)
    {
        return await unitOfWork.BookingRepository
            .FirstOrDefaultAsync(
                QueryTrackerBehavior.NoTracking,
                e => e.Id == bookingId,
                ct)
            ?? throw new BookingNotFoundException(bookingId);
    }
}
