using EventService.Application.Interfaces;
using EventService.Application.Messaging.Handlers;
using EventService.Infrastructure.Persistence;
using Messaging.Abstractions;
using Messaging.Abstractions.Constants;
using Messaging.Abstractions.Contracts.Constants;
using Messaging.Inbox;
using Messaging.Kafka;
using Messaging.Kafka.Models;
using Messaging.Outbox;
using Messaging.Persistence.EfCore.Extensions;
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
        services.AddOutboxRepositories<AppDbContext>();

        services.AddSingleton<IMessageProducer, KafkaProducer>();
        services.AddUnitOfWorkWithOutbox<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddKafkaConsumers(new KafkaConsumerRegistry
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
                        [Commands.ReleaseSeat] = HandlerType.From<ReleaseSeatHandler>(),
                    }
                }
            ]
        });

        services.AddHostedService<InboxSweeper>();

        return services;
    }
}
