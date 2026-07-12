using BookingApi.Domain.Constants;
using BookingApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingApi.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(e => e.Id)
            .HasName("pk_events");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(EventConstants.TitleMaxLength);

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(EventConstants.DescriptionMaxLength);

        builder.Property(e => e.TotalSeats)
            .HasColumnName("total_seats")
            .IsRequired();

        builder.Property(e => e.AvailableSeats)
            .HasColumnName("available_seats")
            .IsRequired();

        builder.Property(e => e.StartAt)
            .HasColumnName("start_at")
            .IsRequired();

        builder.Property(e => e.EndAt)
            .HasColumnName("end_at")
            .IsRequired();

        builder.HasMany(e => e.Bookings)
            .WithOne(e => e.Event)
            .HasForeignKey(e => e.EventId)
            .HasConstraintName("fk_events_bookings")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(e => e.Title)
            .HasDatabaseName("ix_events_title");

        builder.HasIndex(e => new { e.StartAt, e.EndAt })
            .HasDatabaseName("ix_events_start_at_end_at");

        builder.HasIndex(e => e.AvailableSeats)
            .HasDatabaseName("ix_events_available_seats");

        builder.ToTable(tb => tb.HasCheckConstraint(
            "ck_total_seats",
            $"total_seats >= {EventConstants.MinTotalSeats}"));

        builder.ToTable(tb => tb.HasCheckConstraint(
            "ck_available_seats",
            "available_seats <= total_seats"));

        builder.ToTable(tb => tb.HasCheckConstraint(
            "ck_event_dates",
            "end_at > start_at"));

        builder.ToTable(tb => tb.HasCheckConstraint(
            "ck_event_seats",
            "available_seats >= 0 AND available_seats <= total_seats"));
    }
}
