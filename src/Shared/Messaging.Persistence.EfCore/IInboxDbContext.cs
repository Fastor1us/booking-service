using Messaging.Abstractions.Inbox;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Persistence.EfCore;

public interface IInboxDbContext
{
    DbSet<InboxMessage> InboxMessages { get; }
}
