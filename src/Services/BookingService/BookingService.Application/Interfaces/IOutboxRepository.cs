using Messaging.Abstractions.Outbox;

namespace BookingService.Application.Interfaces;

public interface IOutboxRepository : IRepository<OutboxMessage>
{
}
