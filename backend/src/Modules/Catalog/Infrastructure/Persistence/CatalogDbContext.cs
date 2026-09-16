using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.BuildingBlocks.Infrastructure;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext : AppDbContextBase, ICatalogUnitOfWork
{
    public const string Schema = "catalog";

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options, IPublisher publisher)
        : base(options, publisher)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

    /// <summary>
    /// Never called from C# — exists purely so EF Core can translate a call to it into Postgres's
    /// real `unaccent(text)` SQL function (from the `unaccent` extension, enabled by a migration).
    /// Lets keyword search match "áo" against a name stored as "Ao" and vice versa.
    /// </summary>
    public static string Unaccent(string input) => throw new NotSupportedException("Only for use in LINQ queries translated to SQL.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        // Without an explicit schema, EF defaults a mapped DbFunction to this context's default
        // schema ("catalog") — but `CREATE EXTENSION unaccent` installs the real function into
        // "public", so it must be called out explicitly here.
        modelBuilder.HasDbFunction(typeof(CatalogDbContext).GetMethod(nameof(Unaccent))!).HasName("unaccent").HasSchema("public");
        base.OnModelCreating(modelBuilder);
    }
}
