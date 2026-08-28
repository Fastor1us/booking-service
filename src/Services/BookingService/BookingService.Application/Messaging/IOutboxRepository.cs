using BookingService.Application.Interfaces;

namespace BookingService.Application.Messaging;

public interface IOutboxRepository : IRepository<OutboxMessage>
{
}
