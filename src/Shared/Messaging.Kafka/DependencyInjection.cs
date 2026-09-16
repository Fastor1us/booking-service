using Messaging.Kafka.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Kafka;

public static class DependencyInjection
{
    public static IServiceCollection AddKafkaConsumers(
        this IServiceCollection services,
        KafkaConsumerRegistry registry)
    {
        var handlerTypes = registry.Workers
            .SelectMany(w => w.Handlers.Values)
            .Select(ht => ht.Value)
            .Distinct();

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);
        }

        services.AddSingleton(registry);

        services.AddHostedService<KafkaConsumerHost>();

        return services;
    }
}
