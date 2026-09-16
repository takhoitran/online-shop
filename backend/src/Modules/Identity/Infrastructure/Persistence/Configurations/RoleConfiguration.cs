using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Identity.Domain;

namespace OnlineShop.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.HasIndex(r => r.Name).IsUnique();

        // Fixed, well-known ids so every environment seeds the same rows (see IdentityDbContext seeding).
        builder.HasData(
            SeedRole("11111111-1111-1111-1111-111111111111", RoleNames.Seller),
            SeedRole("22222222-2222-2222-2222-222222222222", RoleNames.Buyer),
            SeedRole("33333333-3333-3333-3333-333333333333", RoleNames.Admin));
    }

    private static object SeedRole(string id, string name) => new
    {
        Id = Guid.Parse(id),
        Name = name
    };
}
