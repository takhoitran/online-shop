using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("wishlist_items");
        builder.HasKey(i => i.Id);

        // Reached only through Wishlist's private collection, never repository.Add()'d directly —
        // see the EF Core Added-vs-Modified gotcha documented on InventoryTransactionConfiguration.
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(i => i.AddedAtUtc).HasColumnName("added_at_utc");

        builder.Property<Guid>("WishlistId").HasColumnName("wishlist_id");
        builder.HasIndex("WishlistId");
    }
}
