using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.ToTable("order_details");
        builder.HasKey(d => d.Id);

        // Reached only through Order's private collection — see the EF Core Added-vs-Modified
        // gotcha documented on InventoryTransactionConfiguration.
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(d => d.ProductName).HasColumnName("product_name").HasMaxLength(200).IsRequired();
        builder.Property(d => d.Quantity).HasColumnName("quantity").IsRequired();

        builder.OwnsOne(d => d.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("unit_price_amount").HasColumnType("numeric(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("unit_price_currency").HasMaxLength(10).IsRequired();
        });
        builder.Navigation(d => d.UnitPrice).IsRequired();

        builder.Ignore(d => d.LineTotal);

        builder.Property<Guid>("OrderId").HasColumnName("order_id");
        builder.HasIndex("OrderId");
    }
}
