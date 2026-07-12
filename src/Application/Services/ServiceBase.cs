using BookingApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Application.Services;

public abstract class ServiceBase(AppDbContext context)
{
    protected readonly AppDbContext context = context;

    /// <summary>
    /// Execute operation with retries. 
    /// Notice: context's ChangeTracker being clear every retry
    /// </summary>
    protected async Task ExecuteWithRetryAsync(
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
    protected async Task<T> ExecuteWithRetryAsync<T>(
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
}
