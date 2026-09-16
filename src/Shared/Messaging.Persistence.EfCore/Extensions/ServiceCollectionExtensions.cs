using Messaging.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Persistence.EfCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxRepositories<TContext>(
        this IServiceCollection services)
        where TContext : DbContext, IOutboxDbContext, IInboxDbContext
    {
        services.AddScoped<IOutboxRepository, OutboxRepository<TContext>>();
        services.AddScoped<IOutboxDeadLetterRepository, OutboxDeadLetterRepository<TContext>>();
        services.AddScoped<IInboxRepository, InboxRepository<TContext>>();
        return services;
    }
}
