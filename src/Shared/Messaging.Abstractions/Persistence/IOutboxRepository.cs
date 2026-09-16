using Messaging.Abstractions.Outbox;

namespace Messaging.Abstractions.Persistence;

public interface IOutboxRepository : IRepository<OutboxMessage>
{
}
