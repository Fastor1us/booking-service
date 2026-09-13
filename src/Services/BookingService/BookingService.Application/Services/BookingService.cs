using BookingService.Application.Interfaces;
using BookingService.Domain.Exceptions;
using BookingService.Domain.Models;
using System.Text.Json;
using Messaging.Kafka.Constants;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Contracts.Commands;
using Messaging.Abstractions.Outbox;

namespace BookingService.Application.Services;

public class BookingService(IUnitOfWork unitOfWork) : IBookingService
{
    public async Task<Booking> AddAsync(
        Guid eventId,
        Guid userId,
        CancellationToken ct)
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        unitOfWork.BookingRepository.Add(booking);

        var reserveCommand = new ReserveEventSeat(
            BookingId: booking.Id,
            UserId: booking.UserId,
            EventId: booking.EventId);

        Guid correlationId = Guid.NewGuid();

        unitOfWork.OutboxRepository.Add(new OutboxMessage
        {
            Id = correlationId,
            Topic = Topics.BookingCommandsTopic, // TODO Messaging.Kafka.Constants
            Key = booking.EventId.ToString(),
            MessageType = Commands.ReserveSeat,
            CorrelationId = correlationId,
            Payload = JsonSerializer.Serialize(reserveCommand)
        });

        await unitOfWork.SaveChangesAsync(ct);

        return booking;
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
        throw new NotImplementedException();
    }
}
