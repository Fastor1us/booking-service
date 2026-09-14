using System.Data;

namespace Messaging.Abstractions.Persistence;

public interface IUnitOfWorkBase : IDisposable, IAsyncDisposable
{
    IOutboxRepository OutboxRepository { get; }
    IOutboxDeadLetterRepository OutboxDeadLetterRepository { get; }

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
