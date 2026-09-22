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

    // Circuit breaker state is intentionally not synchronized.
    // Races here can only shift when the circuit opens/closes by a few
    // milliseconds, which is harmless for a 5-second cool-down.
    private DateTimeOffset _circuitOpenedAt = DateTimeOffset.MinValue;
    private volatile bool _circuitOpen;
    private static readonly TimeSpan CircuitBreakDuration = TimeSpan.FromSeconds(5);
    private const int FailureThreshold = 5;
    private int _consecutiveFailures;

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
        if (IsCircuitOpen($"GET {key}")) return default;

        try
        {
            var cache = cm.GetDatabase();
            RedisValue value = await cache.StringGetAsync(key);

            OnRedisSuccess();

            return value.HasValue
                ? JsonSerializer.Deserialize<T>(value.ToString())
                : default;
        }
        catch (Exception ex)
        {
            OnRedisFailure(ex, $"GET {key}");
            return default;
        }
    }

    private async Task SetSafeAsync<T>(string key, T value, TimeSpan ttl)
    {
        if (IsCircuitOpen($"SET {key}")) return;

        try
        {
            var cache = cm.GetDatabase();
            var json = JsonSerializer.Serialize(value);
            await cache.StringSetAsync(key, json, ttl);

            OnRedisSuccess();
        }
        catch (Exception ex)
        {
            OnRedisFailure(ex, $"SET {key}");
        }
    }

    private async Task RemoveSafeAsync(string key)
    {
        if (IsCircuitOpen($"REMOVE {key}")) return;

        try
        {
            var cache = cm.GetDatabase();
            await cache.KeyDeleteAsync(key);

            OnRedisSuccess();
        }
        catch (Exception ex)
        {
            OnRedisFailure(ex, $"REMOVE {key}");
        }
    }

    private bool IsCircuitOpen(string operation)
    {
        if (_circuitOpen && DateTime.UtcNow - _circuitOpenedAt < CircuitBreakDuration)
        {
            _logger.Warn(
                "Redis {Operation} skipped: circuit is open for {Remaining}s more.",
                operation,
                (int)(CircuitBreakDuration - (DateTime.UtcNow - _circuitOpenedAt)).TotalSeconds);
            return true;
        }

        _circuitOpen = false;
        return false;
    }

    private void OnRedisFailure(Exception ex, string operation)
    {
        var failures = Interlocked.Increment(ref _consecutiveFailures);

        if (failures >= FailureThreshold)
        {
            _circuitOpen = true;
            _circuitOpenedAt = DateTimeOffset.UtcNow;
            _logger.Error(ex, "Redis {Operation} failed {Count} times in a row. Opening circuit.",
                operation, failures);
        }
        else
        {
            _logger.Warn(ex, "Redis {Operation} failed ({Count}/{Threshold}).",
                operation, failures, FailureThreshold);
        }
    }

    private void OnRedisSuccess()
    {
        if (Volatile.Read(ref _consecutiveFailures) != 0)
            Interlocked.Exchange(ref _consecutiveFailures, 0);
    }
}

internal sealed class LockEntry
{
    public SemaphoreSlim Semaphore { get; } = new(1, 1);
    public int RefCount;
}
