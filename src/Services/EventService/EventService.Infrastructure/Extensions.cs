using EventService.Application.Interfaces;
using EventService.Application.Messaging.Handlers;
using EventService.Application.Options;
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

            options.ConnectTimeout = 5000;
            options.SyncTimeout = 3000;
            options.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(options);
        });
        services.AddOptions<EventCacheOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                var section = configuration.GetSection("EventCacheOptions");

                Console.WriteLine($"[DIAG] Section exists: {section.Exists()}");
                Console.WriteLine($"[DIAG] EventTtl raw: '{section["EventTtl"]}'");
                Console.WriteLine($"[DIAG] TopEventsTtl raw: '{section["TopEventsTtl"]}'");
                Console.WriteLine($"[DIAG] All keys: {string.Join(", ", section.AsEnumerable().Select(kv => $"{kv.Key}={kv.Value}"))}");

                section.Bind(options);

                Console.WriteLine($"[DIAG] Bound EventTtl: {options.EventTtl}");
                Console.WriteLine($"[DIAG] Bound TopEventsTtl: {options.TopEventsTtl}");
            });
        //services.AddOptions<EventCacheOptions>()
        //    .Configure<IConfiguration>((options, configuration) =>
        //    {
        //        var eventCacheOptions = configuration.GetSection("EventCacheOptions");
        //        configuration.GetSection("EventCacheOptions").Bind(options);
        //    });
        services.AddSingleton<IEventCache, RedisCache>();

        return services;
    }
}
