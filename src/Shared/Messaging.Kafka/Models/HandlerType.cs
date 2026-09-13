using Messaging.Abstractions;

namespace Messaging.Kafka.Models;

public readonly struct HandlerType
{
    public Type Value { get; }

    private HandlerType(Type value) => Value = value;

    public static HandlerType From<T>() where T : IMessageHandler
        => new(typeof(T));

    public static HandlerType From(Type type)
    {
        if (!typeof(IMessageHandler).IsAssignableFrom(type))
            throw new ArgumentException(
                $"{type.FullName} must implement {nameof(IMessageHandler)}",
                nameof(type));

        return new(type);
    }
}

