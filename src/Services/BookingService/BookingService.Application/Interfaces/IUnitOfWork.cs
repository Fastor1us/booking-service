using Messaging.Abstractions.Persistence;

namespace BookingService.Application.Interfaces;

public interface IUnitOfWork : IUnitOfWorkBase
{
    IBookingRepository Bookings { get; }
}
