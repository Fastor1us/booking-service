namespace Messaging.Abstractions;

public interface IMessageProducer
{
    Task ProduceAsync(
        string topic,
        string key,
        string messageType,
        string payload,
        CancellationToken ct);
}
