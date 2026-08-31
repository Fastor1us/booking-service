namespace Messaging.Abstractions;

public interface IMessageHandler
{
    Task HandleAsync(string payload, CancellationToken cancellationToken);
}
