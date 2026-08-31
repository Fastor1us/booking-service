using BookingService.Infrastructure.Persistence;
using Confluent.Kafka;
using Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.BackgroundServices;

public sealed class OutboxRelay(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxRelay> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
    private readonly int _batchSize = 10;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation("Booking Outbox publisher started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();
                var messageProducer = scope.ServiceProvider
                    .GetRequiredService<IMessageProducer>();

                await ProcessBatchAsync(
                    dbContext, messageProducer, stoppingToken);

                await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Unexpected error in Outbox publisher.");

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        logger.LogInformation("Booking Outbox publisher stopped.");
    }

    public async Task ProcessBatchAsync(
        AppDbContext context,
        IMessageProducer producer,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var messages = await context.OutboxMessages
            .OrderBy(e => e.PublishedAtUtc)
            .Take(_batchSize)
            .ToListAsync(ct);

        List<Guid> publishedIds = [];

        foreach (var message in messages)
        {
            try
            {
                await producer.ProduceAsync(
                    topic: message.Topic,
                    key: message.Key,
                    messageType: message.MessageType,
                    payload: message.Payload,
                    ct);

                logger.LogInformation(
                    "Publish Outbox message {id}",
                    message.Id);

                // TODO удалять и при ошибке, но
                // 1) с помещением инфы в dead message таблицу
                // 2) устновки статуса Rejected
                publishedIds.Add(message.Id);
            }
            catch (ProduceException<string, string> ex)
            {
                logger.LogError(
                    "Publish failed. Code={Code}, Reason={Reason}, IsFatal={IsFatal}, DeliveryResult={DeliveryResult}",
                    ex.Error.Code, ex.Error.Reason, ex.Error.IsFatal, ex.DeliveryResult);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{ex.Message}");
                // добавить систему retry - вдруг брокер не может принимать сообщения
                // если было много неудачных попыток, то помещать в очередь бракованных отправок
                // и выставлять Booking.Status = Rejected

                // TODO: добавить причину Rejected в доменную модель

                // TODO: помещать в таблицу outbox-failed DLQ
            }
            
        }

        await context.OutboxMessages
            .Where(m => publishedIds.Contains(m.Id))
            .ExecuteDeleteAsync(ct);
    }
}
