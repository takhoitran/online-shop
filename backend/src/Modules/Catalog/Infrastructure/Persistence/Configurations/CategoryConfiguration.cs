using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Name).IsUnique();

        builder.Property(c => c.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(c => c.NameEn).HasColumnName("name_en").HasMaxLength(100);
        builder.Property(c => c.NameVi).HasColumnName("name_vi").HasMaxLength(100);
        builder.Property(c => c.DescriptionEn).HasColumnName("description_en").HasMaxLength(500);
        builder.Property(c => c.DescriptionVi).HasColumnName("description_vi").HasMaxLength(500);

        builder.Property<uint>("Version").IsRowVersion();
        builder.Ignore(c => c.DomainEvents);
    }
}
