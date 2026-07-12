using BookingApi.Application.Interfaces;
using BookingApi.Domain.Exceptions;
using BookingApi.Domain.Models;
using BookingApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Application.Services;

public class BookingService(AppDbContext context)
    : ServiceBase(context), IBookingService
{
    public async Task<Booking> CreateAsync(Guid eventId, CancellationToken ct)
    {
        var res = await ExecuteWithRetryAsync<Booking>(async _ =>
            {
                var @event = await context.Events
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

                context.Bookings.Add(booking);
                await context.SaveChangesAsync(ct);

                return booking;
            },
            ct);

        return res;
    }

    public async Task<Booking> GetByIdAsync(Guid bookingId, CancellationToken ct)
    {
        return await context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == bookingId, ct)
            ?? throw new BookingNotFoundException(bookingId);
    }
}
