using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Messaging.Abstractions;
using Messaging.Kafka.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text;

namespace Messaging.Kafka;

public sealed class KafkaConsumerHost(
    IOptions<KafkaOptions> options,
    KafkaConsumerRegistry registry,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken st)
    {
        var tasks = registry.Workers
            .Select(sub => RunSubscriptionAsync(sub, st))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    private async Task RunSubscriptionAsync(KafkaWorker worker, CancellationToken st)
    {
        await EnsureTopicExistsAsync(worker.Topic, st);

        var config = new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = worker.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe(worker.Topic);

        try
        {
            while (!st.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(st);

                    //var msgInfo = $"[{result.TopicPartitionOffset}] " +
                    //              $"Key: {result.Message.Key} " +
                    //              $"Value: {result.Message.Value}";
                    //Console.WriteLine(msgInfo);

                    var messageTypeHeader = result.Message.Headers
                        .FirstOrDefault(h => h.Key == Headers.MessageType);
                    var correlationIdHeader = result.Message.Headers
                        .FirstOrDefault(h => h.Key == Headers.CorrelationId);

                    if (messageTypeHeader == null || correlationIdHeader == null)
                    {
                        // TODO логируем Error
                        return;
                    }

                    var messageType = Encoding.UTF8
                        .GetString(messageTypeHeader.GetValueBytes());
                    var correlationId = Encoding.UTF8
                        .GetString(correlationIdHeader.GetValueBytes());

                    if (worker.Handlers.TryGetValue(messageType, out var handlerType))
                    {
                        using var scope = scopeFactory.CreateScope();
                        var handler = (IMessageHandler)scope.ServiceProvider
                            .GetRequiredService(handlerType.Value);
                        await handler.HandleAsync(
                            Guid.Parse(correlationId), result.Message.Value, st);

                        consumer.Commit(result);
                    }
                    else
                    {
                        // TODO логируем Error

                        consumer.Commit(result);
                    }
                }
                catch (ConsumeException ex)
                {
                    // send to DLQ
                    Console.WriteLine($"Ошибка при получении сообщения: {ex.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task EnsureTopicExistsAsync(string topic, CancellationToken ct)
    {
        using var admin = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = options.Value.BootstrapServers
        }).Build();

        try
        {
            var metadata = admin.GetMetadata(topic, TimeSpan.FromSeconds(5));
            if (metadata.Topics.Any(t => t.Topic == topic))
                return;
        }
        catch (KafkaException) { }

        await admin.CreateTopicsAsync(
        [
            new TopicSpecification
            {
                Name = topic,
                NumPartitions = 1,
                ReplicationFactor = 1
            }
        ], new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(10) })
            .WaitAsync(ct); ;
    }
}
