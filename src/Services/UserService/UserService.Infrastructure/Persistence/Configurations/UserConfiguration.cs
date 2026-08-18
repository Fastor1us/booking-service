using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Constants;
using UserService.Domain.Models;

namespace UserService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(e => e.Id)
            .HasName("pk_users");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(e => e.Login)
            .HasColumnName("login")
            .IsRequired()
            .HasMaxLength(UserConstant.LoginMaxLength);

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("bytea");

        builder.Property(e => e.Role)
            .HasColumnName("role")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(e => e.Login)
            .IsUnique()
            .HasDatabaseName("ix_users_login");

        builder.ToTable(tb => tb.HasCheckConstraint(
            "ck_login",
            $"LENGTH(login) >= {UserConstant.LoginMinLength}"));

        builder.ToTable(tb => tb.HasCheckConstraint(
            "ck_users_role_valid",
            $"role IN ({string.Join(", ", Enum.GetNames<UserRole>().Select(s => $"'{s}'"))})"));
    }
}
