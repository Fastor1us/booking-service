using Messaging.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Persistence.EfCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessagingOutbox<TContext>(
        this IServiceCollection services)
        where TContext : DbContext, IOutboxDbContext
    {
        services.AddScoped<IOutboxRepository, OutboxRepository<TContext>>();
        services.AddScoped<IOutboxDeadLetterRepository, OutboxDeadLetterRepository<TContext>>();
        return services;
    }

    //public static IServiceCollection AddMessagingInbox<TContext>(
    //    this IServiceCollection services)
    //    where TContext : DbContext, IInboxDbContext
    //{
    //    services.AddScoped<IInboxRepository, InboxRepository<TContext>>();
    //    return services;
    //}
}
