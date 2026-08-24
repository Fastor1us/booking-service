using BookingService.Infrastructure.Persistence;
using Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.Messaging;

public sealed class OutboxProcessor(
    AppDbContext dbContext,
    IMessageProducer producer,
    ILogger<OutboxProcessor> logger)
{
    public async Task ProcessBatchAsync(
        int batchSize = 10,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var messages = await dbContext.OutboxMessages
            .OrderBy(e => e.PublishedAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);

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

                logger.LogInformation("Publish Outbox message {id} at {time}",
                    message.Id, DateTimeOffset.UtcNow);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to publish Outbox message {MessageId}",
                    message.Id);
            }
            dbContext.OutboxMessages.Remove(message);
            await dbContext.SaveChangesAsync(ct);
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
