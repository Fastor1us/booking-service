using EventService.Application.Interfaces;
using NLog;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace EventService.Infrastructure.Caching;

public class RedisCache(IConnectionMultiplexer cm) : IEventCache
{
    private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

    private static readonly ConcurrentDictionary<string, LockEntry> Locks = new();

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan ttl,
        CancellationToken ct) where T : class?
    {
        var cached = await GetSafeAsync<T>(key);
        if (cached is not null)
            return cached;

        LockEntry entry = Locks.GetOrAdd(key, _ => new LockEntry());
        Interlocked.Increment(ref entry.RefCount);

        try
        {
            await entry.Semaphore.WaitAsync(ct);
            try
            {
                cached = await GetSafeAsync<T>(key);
                if (cached is not null)
                    return cached;

                var value = await factory();
                await SetSafeAsync(key, value, ttl);
                return value;
            }
            finally
            {
                entry.Semaphore.Release();
            }
        }
        finally
        {
            if (Interlocked.Decrement(ref entry.RefCount) == 0)
            {
                Locks.Remove(key, out var _);
            }
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl) where T : class?
    {
        await SetSafeAsync(key, value, ttl);
    }

    public Task RemoveAsync(
        string key,
        CancellationToken ct)
    {
        return RemoveSafeAsync(key);
    }

    private async Task<T?> GetSafeAsync<T>(string key)
    {
        try
        {
            var cache = cm.GetDatabase();

            RedisValue value = await cache.StringGetAsync(key);

            if (value.HasValue)
            {
                return JsonSerializer.Deserialize<T>(value.ToString());
            }
            else
            {
                return default;
            }
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Redis GET failed for key {Key}", key);
            return default;
        }
    }

    private async Task SetSafeAsync<T>(string key, T value, TimeSpan ttl)
    {
        try
        {
            var cache = cm.GetDatabase();

            var json = JsonSerializer.Serialize(value);
            await cache.StringSetAsync(key, json, ttl);
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Redis SET failed for key {Key}", key);
        }
    }

    private async Task RemoveSafeAsync(string key)
    {
        try
        {
            var cache = cm.GetDatabase();

            await cache.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Redis REMOVE failed for key {Key}", key);
        }
    }
}

internal sealed class LockEntry
{
    public SemaphoreSlim Semaphore { get; } = new(1, 1);
    public int RefCount;
}
