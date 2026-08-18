namespace EventService.Application.Interfaces;

/// <summary>
///     <para>
///         A facade that provides access to multiple repositories and coordinates their operations
///         within a single database transaction.
///     </para>
///     <para>
///         The Unit of Work pattern ensures that all repository operations in a business transaction
///         are committed atomically - either all succeed or none are applied.
///     </para>
/// </summary>
/// <remarks>
/// <para>
///     IMPORTANT: IUnitOfWork is NOT thread-safe.
///     It should be used within a single thread and a single scope.
///     For web applications, use Scoped lifetime - one instance per HTTP request.
///     Do not share instances across parallel operations.
/// </para>
/// <para>
///     Two ways to work with transactions:
/// </para>
/// <list type="number">
///     <item>
///        <description>Explicit transaction - use BeginTransactionAsync and CommitTransactionAsync.</description>
///     </item>
///     <item>
///         <description>Implicit transaction with retry - use ExecuteWithRetryAsync for optimistic concurrency.</description>
///     </item>
/// </list>
/// </remarks>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IEventRepository EventRepository { get; }

    Task BeginTransactionAsync(
        System.Data.IsolationLevel isolationLevel,
        CancellationToken ct = default
    );

    Task CommitTransactionAsync(CancellationToken ct = default);

    Task RollbackTransactionAsync(CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task ExecuteWithRetryAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken ct,
        int maxRetries = 3);

    Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct,
        int maxRetries = 3);
}
