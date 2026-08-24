using BookingService.Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Persistence.Configurations;

public sealed class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(e => e.Id)
            .HasName("pk_outbox_messages");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(e => e.Topic)
            .HasColumnName("topic")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Key)
            .HasColumnName("message_key")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.MessageType)
            .HasColumnName("message_type")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(e => e.CorrelationId)
            .HasColumnName("correlation_id")
            .IsRequired();

        builder.Property(e => e.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.PublishedAtUtc)
            .HasColumnName("published_at_utc");

        builder.HasIndex(e => new
        {
            e.PublishedAtUtc,
            e.Id
        })
            .HasDatabaseName("ix_outbox_messages_pending");
    }
}
