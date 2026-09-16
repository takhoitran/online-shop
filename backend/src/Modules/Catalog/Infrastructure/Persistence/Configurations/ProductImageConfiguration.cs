using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");
        builder.HasKey(i => i.Id);

        // Reached only through Product's private collection, never repository.Add()'d directly —
        // see the EF Core Added-vs-Modified gotcha documented on InventoryTransactionConfiguration.
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Url).HasColumnName("url").HasMaxLength(500).IsRequired();
        builder.Property(i => i.SortOrder).HasColumnName("sort_order").IsRequired();
    }
}
