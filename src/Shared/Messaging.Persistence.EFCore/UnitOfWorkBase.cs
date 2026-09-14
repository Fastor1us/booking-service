using Messaging.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Messaging.Persistence.EfCore;

public abstract class UnitOfWorkBase(
    DbContext context,
    IOutboxRepository outboxRepository,
    IOutboxDeadLetterRepository outboxDeadLetterRepository) : IUnitOfWorkBase
{
    public IOutboxRepository OutboxRepository => outboxRepository;
    public IOutboxDeadLetterRepository OutboxDeadLetterRepository => outboxDeadLetterRepository;

    private IDbContextTransaction? _transaction = null;

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel, 
        CancellationToken ct = default)
    {
        _transaction = await context.Database
            .BeginTransactionAsync(isolationLevel, ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Execute operation with retries. 
    /// Notice: context's ChangeTracker being clear every retry
    /// </summary>
    public async Task ExecuteWithRetryAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken ct,
        int maxRetries = 3)
    {
        var attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                await operation(ct);
                return;
            }
            catch (DbUpdateConcurrencyException) when (attempt < maxRetries - 1)
            {
                attempt++;
                context.ChangeTracker.Clear();
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Execute operation with retries. 
    /// Notice: context's ChangeTracker being clear every retry
    /// </summary>
    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct,
        int maxRetries = 3)
    {
        var attempt = 0;

        while (attempt < maxRetries)
        {
            try
            {
                return await operation(ct);
            }
            catch (DbUpdateConcurrencyException) when (attempt < maxRetries - 1)
            {
                attempt++;
                context.ChangeTracker.Clear();
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        throw new InvalidOperationException("Should never reach here");
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await context.DisposeAsync();
    }
}
