using Messaging.Abstractions.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Persistence.EfCore;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<OutboxDeadLetter> OutboxDeadLetters { get; }
}
