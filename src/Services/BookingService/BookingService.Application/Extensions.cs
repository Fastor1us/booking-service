using BookingService.Application.Interfaces;
using BookingService.Application.Services;
using Messaging.Abstractions.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, Services.BookingService>();
        services.AddScoped<IOutboxCompensator, OutboxCompensator>();
        services.AddScoped<IOutboxStore, OutboxStore>();

        return services;
    }
}
