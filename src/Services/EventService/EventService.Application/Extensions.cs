using EventService.Application.Interfaces;
using EventService.Application.Services;
using Messaging.Abstractions.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, Services.EventService>();
        services.AddScoped<IOutboxCompensator, OutboxCompensator>();

        return services;
    }
}
