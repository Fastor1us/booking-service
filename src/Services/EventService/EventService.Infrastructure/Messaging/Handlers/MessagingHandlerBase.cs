using Messaging.Abstractions;
using System.Runtime.Serialization;
using System.Text.Json;

namespace EventService.Infrastructure.Messaging.Handlers;

public abstract class MessagingHandlerBase<T> : IMessageHandler
{
    public abstract Task HandleAsync(Guid correlationId, string payload, CancellationToken ct);

    protected T GetCommand(string payload)
    {
        return JsonSerializer.Deserialize<T>(payload)
            ?? throw new SerializationException($"Is not able payload to serialize to {typeof(T)}");
    }
}
