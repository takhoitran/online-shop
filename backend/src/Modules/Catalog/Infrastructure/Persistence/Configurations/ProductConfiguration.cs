using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.HasIndex(p => p.Name);

        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(p => p.NameEn).HasColumnName("name_en").HasMaxLength(200);
        builder.Property(p => p.NameVi).HasColumnName("name_vi").HasMaxLength(200);
        builder.Property(p => p.DescriptionEn).HasColumnName("description_en").HasMaxLength(2000);
        builder.Property(p => p.DescriptionVi).HasColumnName("description_vi").HasMaxLength(2000);

        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(m => m.Amount).HasColumnName("price_amount").HasColumnType("numeric(18,2)").IsRequired();
            price.Property(m => m.Currency).HasColumnName("price_currency").HasMaxLength(10).IsRequired();
        });
        builder.Navigation(p => p.Price).IsRequired();

        builder.Property(p => p.CategoryId).HasColumnName("category_id").IsRequired();
        builder.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => p.CategoryId);

        builder.Property(p => p.ImageUrl).HasColumnName("image_url").HasMaxLength(500);

        builder.Property(p => p.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0).IsRequired();

        builder.Property(p => p.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(p => p.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasMany(p => p.Images).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Images).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<uint>("Version").IsRowVersion();
        builder.Ignore(p => p.DomainEvents);
    }
}
