using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(c => c.UserId).IsUnique();

        builder.HasMany(c => c.Items).WithOne().HasForeignKey("CartId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<uint>("Version").IsRowVersion();
        builder.Ignore(c => c.DomainEvents);
    }
}
