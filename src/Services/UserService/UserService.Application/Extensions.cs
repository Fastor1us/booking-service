using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces;

namespace UserService.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, Services.UserService>();

        return services;
    }
}
