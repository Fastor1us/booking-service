using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApi.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
