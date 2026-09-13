using EventService.Application.Interfaces;
using EventService.Infrastructure.Messaging.Handlers;
using EventService.Infrastructure.Persistence;
using Messaging.Abstractions;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Kafka;
using Messaging.Kafka.Constants;
using Messaging.Kafka.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var connectionString = configuration.GetConnectionString("eventsdb")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string is required");

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IEventRepository, Repositories.EventRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        services.AddScoped<ReserveSeatHandler>();
        services.AddSingleton<KafkaConsumerRegistry>(_ =>
        {
            return new KafkaConsumerRegistry
            {
                Workers =
                [
                    new KafkaWorker
                    {
                        Topic = Topics.BookingCommandsTopic,
                        GroupId = GroupIds.EventGroup,
                        Handlers = new Dictionary<string, HandlerType>
                        {
                            [Commands.ReserveSeat] = HandlerType.From<ReserveSeatHandler>(),
                        }
                    }
                ]
            };
        });
        services.AddHostedService<KafkaConsumerHost>();
        services.AddSingleton<IMessageProducer, KafkaProducer>();
        //services.AddHostedService<OutboxRelay>();

        return services;
    }
}
