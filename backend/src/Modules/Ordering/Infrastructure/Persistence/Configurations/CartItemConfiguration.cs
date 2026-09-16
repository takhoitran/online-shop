using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");
        builder.HasKey(i => i.Id);

        // Reached only through Cart's private collection, never repository.Add()'d directly — see
        // the EF Core Added-vs-Modified gotcha documented on InventoryTransactionConfiguration.
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(i => i.Quantity).HasColumnName("quantity").IsRequired();

        builder.Property<Guid>("CartId").HasColumnName("cart_id");
        builder.HasIndex("CartId");
    }
}
