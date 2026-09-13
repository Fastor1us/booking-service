using Messaging.Kafka.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Kafka;

public static class DependencyInjection
{
    public static IServiceCollection AddKafkaConsumerRegistry(
        this IServiceCollection services,
        KafkaConsumerRegistry registry)
    {
        services.AddSingleton(registry);

        return services;
    }
}
