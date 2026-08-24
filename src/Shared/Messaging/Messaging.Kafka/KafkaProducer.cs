using Confluent.Kafka;
using Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Messaging.Kafka;

public sealed class KafkaProducer(IOptions<KafkaOptions> options)
    : IMessageProducer, IDisposable
{
    private readonly IProducer<string, string> producer =
        new ProducerBuilder<string, string>(
            new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                MaxInFlight = 5,
            })
        .Build();

    public async Task ProduceAsync(
        string topic,
        string key,
        string messageType,
        string payload,
        CancellationToken ct)
    {
        var message = new Message<string, string>
        {
            Key = key,
            Value = payload,
            Headers = new Headers
            {
                {
                    "message-type",
                    System.Text.Encoding.UTF8.GetBytes(messageType)
                }
            }
        };

        var result = await producer.ProduceAsync(
            topic,
            message,
            ct);

        if (result.Status != PersistenceStatus.Persisted)
        {
            throw new InvalidOperationException(
                $"Kafka message was not persisted. " +
                $"Topic: {topic}, " +
                $"Partition: {result.Partition}, " +
                $"Offset: {result.Offset}");
        }
    }

    public void Dispose()
    {
        producer.Flush(TimeSpan.FromSeconds(10));
        producer.Dispose();
    }
}
