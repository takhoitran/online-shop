using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Notification.Domain;

namespace OnlineShop.Modules.Notification.Infrastructure.Persistence.Configurations;

internal sealed class AppNotificationConfiguration : IEntityTypeConfiguration<AppNotification>
{
    public void Configure(EntityTypeBuilder<AppNotification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(n => n.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(n => n.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(n => n.Message).HasColumnName("message").HasMaxLength(1000).IsRequired();
        builder.Property(n => n.RelatedOrderId).HasColumnName("related_order_id");
        builder.Property(n => n.RelatedProductId).HasColumnName("related_product_id");
        builder.Property(n => n.IsRead).HasColumnName("is_read").IsRequired();
        builder.Property(n => n.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(n => new { n.UserId, n.CreatedAtUtc });
    }
}
