using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Identity.Domain;
using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .HasConversion(username => username.Value, value => Username.Create(value))
            .HasColumnName("username")
            .HasMaxLength(Username.MaxLength)
            .IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(u => u.RoleId).HasColumnName("role_id").IsRequired();

        builder.Property(u => u.TelegramId).HasColumnName("telegram_id").HasMaxLength(64);
        builder.HasIndex(u => u.TelegramId).IsUnique().HasFilter("telegram_id IS NOT NULL");

        builder.Property(u => u.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(u => u.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne<Role>().WithMany().HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);

        // Optimistic concurrency via Postgres' native xmin system column — no extra write, no
        // CLR property leaking into the Domain layer. Npgsql's provider convention auto-maps any
        // shadow uint property configured IsRowVersion() to xmin (UseXminAsConcurrencyToken was
        // removed in Npgsql.EntityFrameworkCore.PostgreSQL 7+ in favor of this standard EF API).
        builder.Property<uint>("Version").IsRowVersion();

        builder.Ignore(u => u.DomainEvents);
    }
}
