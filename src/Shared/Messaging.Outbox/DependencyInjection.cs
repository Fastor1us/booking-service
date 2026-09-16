using Messaging.Abstractions.Outbox;
using Messaging.Abstractions.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Outbox;

public static class DependencyInjection
{
    public static IServiceCollection AddUnitOfWorkWithOutbox<TType, TImplementation>(
        this IServiceCollection services)
        where TImplementation : class, IUnitOfWorkBase, TType
        where TType : class, IUnitOfWorkBase
    {
        services.AddScoped<TImplementation>();

        services.AddScoped<IUnitOfWorkBase>(sp =>
            sp.GetRequiredService<TImplementation>());

        services.AddScoped<TType>(sp =>
            sp.GetRequiredService<TImplementation>());

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddHostedService<OutboxRelay>();

        return services;
    }
}
