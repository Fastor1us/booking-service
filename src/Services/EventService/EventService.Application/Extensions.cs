using EventService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, Services.EventService>();

        return services;
    }
}
