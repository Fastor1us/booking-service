using BookingService.Infrastructure.Persistence;
using Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.BackgroundServices;

public sealed class OutboxPollerBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPollerBackgroundService> logger)
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
            await producer.ProduceAsync(
                topic: message.Topic,
                key: message.Key,
                messageType: message.MessageType,
                payload: message.Payload,
                ct);

            logger.LogInformation(
                "Publish Outbox message {id}",
                message.Id);

            publishedIds.Add(message.Id);
        }

        await context.OutboxMessages
            .Where(m => publishedIds.Contains(m.Id))
            .ExecuteDeleteAsync(ct);
    }
}
