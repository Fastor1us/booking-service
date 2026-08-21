namespace Messaging.Abstractions;

public sealed record MessageEnvelope<T>(
    Guid MessageId,
    string MessageType,
    int Version,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime OccurredAtUtc,
    T Payload);
