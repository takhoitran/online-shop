using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("vouchers");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.HasIndex(v => v.Code).IsUnique();

        builder.Property(v => v.DiscountType).HasColumnName("discount_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(v => v.DiscountValue).HasColumnName("discount_value").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(v => v.MaxUses).HasColumnName("max_uses");
        builder.Property(v => v.UsedCount).HasColumnName("used_count").IsRequired();
        builder.Property(v => v.ExpiresAtUtc).HasColumnName("expires_at_utc");
        builder.Property(v => v.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(v => v.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.Property<uint>("Version").IsRowVersion();
        builder.Ignore(v => v.DomainEvents);
    }
}
