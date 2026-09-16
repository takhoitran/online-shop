using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("wishlists");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(w => w.UserId).IsUnique();

        builder.HasMany(w => w.Items).WithOne().HasForeignKey("WishlistId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(w => w.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<uint>("Version").IsRowVersion();
        builder.Ignore(w => w.DomainEvents);
    }
}
