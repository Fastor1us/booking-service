using System.Data;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace UserService.Infrastructure.UnitOfWork;

public class UnitOfWork(
    AppDbContext context,
    IUserRepository userRepository) : IUnitOfWork
{
    public IUserRepository UserReopitory => userRepository;

    private IDbContextTransaction? _dbContextTransaction = null;

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default)
    {
        _dbContextTransaction = await context.Database.BeginTransactionAsync(isolationLevel, ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_dbContextTransaction != null)
        {
            await _dbContextTransaction.CommitAsync(ct);
            await _dbContextTransaction.DisposeAsync();
            _dbContextTransaction = null;
        }
    }
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_dbContextTransaction != null)
        {
            await _dbContextTransaction.RollbackAsync(ct);
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
        _dbContextTransaction?.Dispose();
        context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContextTransaction != null)
        {
            await _dbContextTransaction.DisposeAsync();
        }
        await context.DisposeAsync();
    }
}
