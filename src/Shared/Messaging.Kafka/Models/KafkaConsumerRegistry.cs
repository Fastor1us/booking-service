    namespace Messaging.Kafka.Models;

public sealed class KafkaConsumerRegistry
{
    public required IReadOnlyList<KafkaWorker> Workers { get; init; }
}
