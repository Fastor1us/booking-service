using Messaging.Abstractions.Outbox;

namespace Messaging.Abstractions.Persistence;

public interface IOutboxDeadLetterRepository : IRepository<OutboxDeadLetter>
{
}
