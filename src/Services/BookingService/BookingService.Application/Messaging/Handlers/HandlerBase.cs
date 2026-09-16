using BookingService.Application.Interfaces;
using Messaging.Abstractions;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Abstractions.Inbox;
using System.Runtime.Serialization;
using System.Text.Json;

namespace BookingService.Application.Messaging.Handlers;

public abstract class HandlerBase(IUnitOfWork unitOfWork) : IMessageHandler
{
    protected IUnitOfWork _unitOfWork = unitOfWork;

    protected async Task<bool> TryRegisterInboxMessageAsync(
        Guid correlationId,
        string messageType,
        string payload,
        CancellationToken ct)
    {
        var existed = await _unitOfWork.InboxMessages
            .FirstOrDefaultAsync(e => e.CorrelationId == correlationId, ct);

        if (existed != null) return false;

        _unitOfWork.InboxMessages.Add(new InboxMessage()
        {
            Id = correlationId,
            CorrelationId = correlationId,
            MessageType = messageType,
            Payload = payload,
            ReceivedAt = DateTime.UtcNow,
        });

        return true;
    }

    protected static T DeserializePayload<T>(string payload)
    {
        return JsonSerializer.Deserialize<T>(payload)
            ?? throw new SerializationException($"Is not able payload to serialize to {typeof(T)}");
    }

    public abstract Task HandleAsync(Guid correlationId, string payload, CancellationToken ct);
}
