using BookingService.Application.Interfaces;
using BookingService.Application.Messaging.Handlers;
using BookingService.Infrastructure.BackgroundServices;
using BookingService.Infrastructure.Persistence;
using BookingService.Infrastructure.Repositories;
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
        services.AddOutboxRepositories<AppDbContext>();

        services.AddSingleton<IMessageProducer, KafkaProducer>();
        services.AddUnitOfWorkWithOutbox<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddKafkaConsumers(new KafkaConsumerRegistry
        {
            Workers =
            [
                new KafkaWorker
                {
                    Topic = Topics.EventEventsTopic,
                    GroupId = GroupIds.BookingGroup,
                    Handlers = new Dictionary<string, HandlerType>
                    {
                        [Events.SeatReserved] = HandlerType.From<SeatReservedHandler>(),
                        [Events.SeatReservationRejected] = HandlerType.From<SeatReservationRejectedHandler>(),
                        [Events.SeatReleased] = HandlerType.From<SeatReleasedHandler>(),
                        [Events.SeatReleasingRejected] = HandlerType.From<SeatReleasingRejectedHandler>(),
                    }
                }
            ]
        });

        services.AddHostedService<BookingMissingAnswerResolver>();
        services.AddHostedService<InboxSweeper>();

        return services;
    }
}
