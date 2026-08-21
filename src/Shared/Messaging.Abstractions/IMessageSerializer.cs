namespace Messaging.Abstractions;

public interface IMessageSerializer
{
    string Serialize<T>(MessageEnvelope<T> message);

    MessageEnvelope<T> Deserialize<T>(string json);
}
