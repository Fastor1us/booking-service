using Messaging.Abstractions;
using Messaging.Abstractions.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Messaging.Outbox;

public sealed class OutboxRelay(
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

    public int BatchSize { private get; init; } = 10;
    public int MaxPublishAttempts { private get; init; } = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
                var compensator = scope.ServiceProvider.GetRequiredService<IOutboxCompensator>();
                var producer = scope.ServiceProvider.GetRequiredService<IMessageProducer>();

                await ProcessBatchAsync(store, compensator, producer, stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error in {Relay}.", typeof(OutboxRelay).Name);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    public async Task ProcessBatchAsync(
        IOutboxStore store,
        IOutboxCompensator compensator,
        IMessageProducer producer,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var messages = await store.GetPendingAsync(BatchSize, now, ct);

        List<Guid> producedIds = [];

        foreach (var message in messages)
        {
            try
            {
                await producer.ProduceAsync(
                    message.Topic, message.Key, message.MessageType,
                    message.CorrelationId, message.Payload, ct);

                _logger.Info("Published outbox message {Id}", message.Id);
                producedIds.Add(message.Id);
            }
            catch (Exception ex)
            {
                message.Errors.Add(ex.Message);
                message.RetryCount++;

                if (message.RetryCount <= MaxPublishAttempts)
                {
                    message.NextAttemptAt = now + TimeSpan
                        .FromSeconds(message.RetryCount * 10);
                }
                else
                {
                    var handled = await compensator
                        .TryCompensateAsync(message, ct);
                    if (!handled)
                    {
                        _logger.Error(
                            "No compensator handled message type '{Type}' (id {Id}).",
                            message.MessageType, message.Id);
                    }

                    await store.MoveToDeadLetterAsync(message, ct);
                }
            }
        }

        if (producedIds.Count > 0)
            await store.MarkAsPublishedAsync(producedIds, ct);

        await store.SaveChangesAsync(ct);
    }
}
