using BookingService.Application.Sagas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Persistence.Configurations;

//public sealed class BookingSagaConfiguration : IEntityTypeConfiguration<BookingSaga>
//{
//    public void Configure(EntityTypeBuilder<BookingSaga> builder)
//    {
//        builder.ToTable("booking_sagas");

//        builder.HasKey(x => x.Id);

//        builder.Property(x => x.State)
//            .HasConversion<string>()
//            .HasMaxLength(100);

//        builder.Property(x => x.FailureReason)
//            .HasMaxLength(500);

//        builder.HasIndex(x => x.BookingId)
//            .IsUnique();

//        builder.HasIndex(x => x.DeadlineUtc);
//    }
//}
