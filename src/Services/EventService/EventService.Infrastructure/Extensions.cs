using EventService.Application.Cache;
using EventService.Application.Interfaces;
using EventService.Application.Messaging.Handlers;
using EventService.Infrastructure.Caching;
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
using StackExchange.Redis;

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

        services.AddOptions<KafkaOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                options.BootstrapServers = configuration.GetConnectionString("Kafka")
                    ?? throw new InvalidOperationException("Connection string is required");
            });

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

        services.AddSingleton<IConnectionMultiplexer>((serviceProvider) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var redisConnection = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Connection string is required");

            var options = ConfigurationOptions.Parse(redisConnection);

            options.ConnectTimeout = 2000;
            options.SyncTimeout = 500;
            options.AsyncTimeout = 500;
            options.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(options);
        });
        services.AddOptions<EventCacheOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                var eventCacheOptions = configuration.GetSection("EventCacheOptions");
                configuration.GetSection("EventCacheOptions").Bind(options);
            });
        services.AddSingleton<IEventCache, RedisCache>();

        return services;
    }
}
