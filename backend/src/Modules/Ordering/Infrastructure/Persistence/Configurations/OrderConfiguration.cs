using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(o => o.UserId);

        builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(o => o.Status);

        builder.Property(o => o.PaymentMethod).HasColumnName("payment_method").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(o => o.PaymentStatus).HasColumnName("payment_status").HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.OwnsOne(o => o.Subtotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("subtotal_amount").HasColumnType("numeric(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("subtotal_currency").HasMaxLength(10).IsRequired();
        });
        builder.Navigation(o => o.Subtotal).IsRequired();

        builder.OwnsOne(o => o.DiscountAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("discount_amount").HasColumnType("numeric(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("discount_currency").HasMaxLength(10).IsRequired();
        });
        builder.Navigation(o => o.DiscountAmount).IsRequired();

        builder.Property(o => o.VoucherCode).HasColumnName("voucher_code").HasMaxLength(50);

        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("total_amount").HasColumnType("numeric(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(10).IsRequired();
        });
        builder.Navigation(o => o.TotalAmount).IsRequired();

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.RecipientName).HasColumnName("shipping_recipient_name").HasMaxLength(200).IsRequired();
            address.Property(a => a.PhoneNumber).HasColumnName("shipping_phone_number").HasMaxLength(30).IsRequired();
            address.Property(a => a.AddressLine).HasColumnName("shipping_address_line").HasMaxLength(300).IsRequired();
            address.Property(a => a.City).HasColumnName("shipping_city").HasMaxLength(100).IsRequired();
        });
        builder.Navigation(o => o.ShippingAddress).IsRequired();

        builder.Property(o => o.OrderDateUtc).HasColumnName("order_date_utc");
        builder.Property(o => o.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasMany(o => o.Details).WithOne().HasForeignKey("OrderId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Details).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<uint>("Version").IsRowVersion();
        builder.Ignore(o => o.DomainEvents);
    }
}
