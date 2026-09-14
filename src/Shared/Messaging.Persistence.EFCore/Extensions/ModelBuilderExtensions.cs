using Messaging.Persistence.EfCore.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Persistence.EfCore.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyOutboxConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxDeadLetterConfiguration());
        return modelBuilder;
    }

    public static ModelBuilder ApplyInboxConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        return modelBuilder;
    }
}
