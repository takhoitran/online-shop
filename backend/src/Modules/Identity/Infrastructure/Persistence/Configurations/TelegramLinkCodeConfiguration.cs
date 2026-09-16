using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Identity.Domain;

namespace OnlineShop.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class TelegramLinkCodeConfiguration : IEntityTypeConfiguration<TelegramLinkCode>
{
    public void Configure(EntityTypeBuilder<TelegramLinkCode> builder)
    {
        builder.ToTable("telegram_link_codes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();

        builder.Property(c => c.Code).HasColumnName("code").HasMaxLength(6).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.ExpiresAtUtc).HasColumnName("expires_at_utc");
        builder.Property(c => c.ConsumedAtUtc).HasColumnName("consumed_at_utc");

        builder.Ignore(c => c.IsExpired);
        builder.Ignore(c => c.IsConsumed);
        builder.Ignore(c => c.DomainEvents);
    }
}
