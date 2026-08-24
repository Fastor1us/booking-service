using BookingService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, Services.BookingService>();

        return services;
    }
}
