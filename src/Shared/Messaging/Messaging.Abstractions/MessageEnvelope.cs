namespace Messaging.Abstractions;

public sealed record MessageEnvelope<T>(
    Guid MessageId,
    string MessageType,
    Guid CorrelationId,
    DateTime OccurredAtUtc,
    T Payload);
