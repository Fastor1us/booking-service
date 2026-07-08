using BookingApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingApi.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(e => e.Id)
            .HasName("pk_bookings");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(e => e.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.HasOne(e => e.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(e => e.EventId)
            .HasConstraintName("fk_bookings_events")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.ProcessedAt)
                   .HasColumnName("processed_at")
                   .IsRequired(false);

        builder.Property(b => b.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(e => e.EventId)
            .HasDatabaseName("ix_bookings_event_id");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("ix_bookings_status");

        builder.HasIndex(e => new { e.EventId, e.Status })
            .HasDatabaseName("ix_bookings_event_id_status");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("ix_bookings_created_at");

        builder.ToTable(tb => tb.HasCheckConstraint(
            "ck_bookings_status_valid",
            "status IN ('Pending', 'Confirmed', 'Rejected')"));
    }
}
