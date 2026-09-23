namespace EventService.Application.Interfaces;

public interface IEventCache
{
    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl) where T : class?;

    public Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan ttl,
        CancellationToken ct) where T : class?;

    public Task RemoveAsync(
        string key,
        CancellationToken ct);
}
