using BookingService.Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Persistence.Configurations;

public sealed class OutboxDeadLetterConfiguration 
    : IEntityTypeConfiguration<OutboxDeadLetter>
{
    public void Configure(EntityTypeBuilder<OutboxDeadLetter> builder)
    {
        builder.ToTable("outbox_dead_letters");

        builder.HasKey(e => e.Id)
            .HasName("pk_outbox_dead_letters");

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

        builder.Property(e => e.MovedToDeadLettersAt)
            .HasColumnName("moved_to_dead_letters_at_utc");

        builder.Property(e => e.Errors)
            .HasColumnName("errors")
            .HasColumnType("text[]");

        builder.Property(b => b.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(e => new
        {
            e.MovedToDeadLettersAt,
            e.Id
        })
            .HasDatabaseName("ix_outbox_dead_letters_moved_at");
    }
}
