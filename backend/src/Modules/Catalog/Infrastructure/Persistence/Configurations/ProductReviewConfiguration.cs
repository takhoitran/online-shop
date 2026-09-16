using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("product_reviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ProductId).HasColumnName("product_id").IsRequired();
        builder.HasOne<Product>().WithMany().HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => r.ProductId);

        builder.Property(r => r.BuyerId).HasColumnName("buyer_id").IsRequired();
        builder.HasIndex(r => new { r.ProductId, r.BuyerId }).IsUnique();

        builder.Property(r => r.BuyerName).HasColumnName("buyer_name").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Rating).HasColumnName("rating").IsRequired();
        builder.Property(r => r.Comment).HasColumnName("comment").HasMaxLength(2000);
        builder.Property(r => r.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.Property<uint>("Version").IsRowVersion();
        builder.Ignore(r => r.DomainEvents);
    }
}
