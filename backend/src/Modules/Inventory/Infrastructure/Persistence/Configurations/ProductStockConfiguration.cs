using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Inventory.Domain;

namespace OnlineShop.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
{
    public void Configure(EntityTypeBuilder<ProductStock> builder)
    {
        builder.ToTable("product_stocks");

        // Id IS the ProductId — see ProductStock's XML doc (one stock record per product).
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("product_id").ValueGeneratedNever();
        builder.Ignore(s => s.ProductId);

        builder.Property(s => s.QuantityOnHand).HasColumnName("quantity_on_hand").IsRequired();

        builder.HasMany(s => s.Transactions)
            .WithOne()
            .HasForeignKey("ProductStockId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Transactions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<uint>("Version").IsRowVersion();
        builder.Ignore(s => s.DomainEvents);
    }
}
