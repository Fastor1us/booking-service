namespace Messaging.Kafka.Models;

public sealed class KafkaWorker
{
    public required IReadOnlyDictionary<string, HandlerType> Handlers { get; init; }
    public required string Topic { get; init; }
    public required string GroupId { get; init; }
}
