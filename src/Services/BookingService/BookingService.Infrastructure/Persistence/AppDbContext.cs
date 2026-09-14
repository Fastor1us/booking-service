using BookingService.Domain.Models;
using Messaging.Abstractions.Inbox;
using Messaging.Abstractions.Outbox;
using Messaging.Persistence.EfCore;
using Messaging.Persistence.EfCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IOutboxDbContext, IInboxDbContext
{
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OutboxDeadLetter> OutboxDeadLetters => Set<OutboxDeadLetter>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.ApplyOutboxConfigurations();
        modelBuilder.ApplyInboxConfigurations();
    }
}
