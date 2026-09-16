using System.Data;

namespace Messaging.Abstractions.Persistence;

public interface IUnitOfWorkBase : IDisposable, IAsyncDisposable
{
    IOutboxRepository OutboxMessages { get; }
    IOutboxDeadLetterRepository OutboxDeadLetters { get; }
    IInboxRepository InboxMessages { get; }

    Task BeginTransactionAsync(CancellationToken ct = default);

    Task BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken ct = default);

    Task CommitTransactionAsync(CancellationToken ct = default);

    Task RollbackTransactionAsync(CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task ExecuteWithRetryAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken ct,
        int maxRetries = 3);

    Task<T> ExecuteWithRetryAsync<T>(Func<CancellationToken,
        Task<T>> operation,
        CancellationToken ct,
        int maxRetries = 3);
}
