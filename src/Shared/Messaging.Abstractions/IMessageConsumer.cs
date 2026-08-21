namespace Messaging.Abstractions;

public interface IMessageConsumer<T>
{
    Task<T?> ConsumeAsync(CancellationToken ct);
}
