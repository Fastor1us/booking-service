using Messaging.Abstractions.Inbox;

namespace Messaging.Abstractions.Persistence;

public interface IInboxRepository : IRepository<InboxMessage>
{
    public Task<int> ExecuteDeleteOutdatedAsync(
        DateTimeOffset threshold,
        CancellationToken ct = default);
}
