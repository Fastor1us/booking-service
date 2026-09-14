using BookingService.Application.Interfaces;
using BookingService.Application.Services;
using BookingService.Infrastructure.Persistence;
using BookingService.Infrastructure.Repositories;
using Messaging.Abstractions;
using Messaging.Abstractions.Outbox;
using Messaging.Kafka;
using Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var connectionString = configuration.GetConnectionString("bookingsdb")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string is required");

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IOutboxDeadLetterRepository, OutboxDeadLetterRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IOutboxCompensator, OutboxCompensator>();

        services.AddSingleton<IMessageProducer, KafkaProducer>();
        services.AddHostedService<OutboxRelay>();

        return services;
    }
}
