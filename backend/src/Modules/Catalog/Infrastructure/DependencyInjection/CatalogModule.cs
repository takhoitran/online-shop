using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;
using OnlineShop.Modules.Catalog.Infrastructure.Persistence;
using OnlineShop.Modules.Catalog.Infrastructure.Persistence.Queries;
using OnlineShop.Modules.Catalog.Infrastructure.Persistence.Repositories;
using OnlineShop.Modules.Catalog.Infrastructure.Storage;

namespace OnlineShop.Modules.Catalog.Infrastructure.DependencyInjection;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));

        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("Postgres"),
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", CatalogDbContext.Schema)));

        services.AddScoped<ICatalogUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
        services.AddScoped<IProductCatalogQueries, ProductCatalogQueries>();
        services.AddScoped<IProductReviewQueries, ProductReviewQueries>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
