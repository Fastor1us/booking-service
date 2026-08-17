using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using UserService.Infrasturcture.Repositories;
using UserService.Infrasturcture.Persistence;
using UserService.Application.Interfaces;
using UserService.Infrasturcture.Security;

namespace UserService.Infrasturcture;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string is required");

            options.UseNpgsql(connectionString);
        });

        services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }
}
