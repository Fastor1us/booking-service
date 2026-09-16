using BookingService.Application.Interfaces;
using BookingService.Infrastructure.Persistence;
using Messaging.Abstractions.Persistence;
using Messaging.Persistence.EfCore;

namespace BookingService.Infrastructure.UnitOfWork;

public class UnitOfWork(
    AppDbContext context,
    IBookingRepository bookingRepository,
    IOutboxRepository outboxRepository,
    IOutboxDeadLetterRepository outboxDeadLetterRepository)
    : UnitOfWorkBase(context, outboxRepository, outboxDeadLetterRepository),
      IUnitOfWork
{
    public IBookingRepository Bookings => bookingRepository;
}
