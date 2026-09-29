using BookingService.Application.Interfaces;
using Messaging.Abstractions;
using Messaging.Abstractions.Inbox;
using Microsoft.Extensions.Logging;
using System.Runtime.Serialization;
using System.Text.Json;

namespace BookingService.Application.Messaging.Handlers;

public abstract class HandlerBase(IUnitOfWork unitOfWork) : IMessageHandler
{
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;
    protected abstract ILogger Logger { get; }

    protected async Task<bool> TryRegisterInboxMessageAsync(
        Guid correlationId,
        string messageType,
        string payload,
        CancellationToken ct)
    {
        var existed = await _unitOfWork.InboxMessages
            .FirstOrDefaultAsync(e => e.CorrelationId == correlationId, ct);

        if (existed != null)
        {
            Logger.LogDebug(
                "Inbox duplicate ignored. CorrelationId={CorrelationId}, MessageType={MessageType}",
                correlationId, messageType);

            return false;
        }

        _unitOfWork.InboxMessages.Add(new InboxMessage
        {
            Id = correlationId,
            CorrelationId = correlationId,
            MessageType = messageType,
            Payload = payload,
            ReceivedAt = DateTime.UtcNow,
        });

        return true;
    }

    protected T DeserializePayload<T>(string payload, Guid correlationId)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payload)
                ?? throw new SerializationException(
                    $"Payload is null after deserialization to {typeof(T).Name}");
        }
        catch (JsonException ex)
        {
            Logger.LogError(
                ex,
                "Failed to deserialize payload to {Type}. CorrelationId={CorrelationId}",
                typeof(T).Name, correlationId);

            throw;
        }
    }

    public abstract Task HandleAsync(Guid correlationId, string payload, CancellationToken ct);
}
