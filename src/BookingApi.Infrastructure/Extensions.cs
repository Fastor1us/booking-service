using BookingApi.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BookingApi.Infrastructure.BackgroundServices;
using BookingApi.Infrastructure.Repositories;
using BookingApi.Infrastructure.UnitOfWork;
using BookingApi.Application.Interfaces;

namespace BookingApi.Infrastructure;

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
            options.UseLazyLoadingProxies();
        });

        services.AddScoped<IEventRepository, EFCoreEventRepository>();
        services.AddScoped<IBookingRepository, EFCoreBookingRepository>();
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

        services.AddHostedService<PendingBookingProcessor>();

        return services;
    }
}
