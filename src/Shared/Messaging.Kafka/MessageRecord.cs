namespace Messaging.Kafka;

public sealed record MessageRecord(
    string Topic,
    int Partition,
    long Offset,
    string Key,
    string MessageType,
    string Payload);
