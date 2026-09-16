using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Modules.Inventory.Application.Abstractions;
using OnlineShop.Modules.Inventory.Domain;
using OnlineShop.Modules.Inventory.Infrastructure.Persistence;
using OnlineShop.Modules.Inventory.Infrastructure.Persistence.Queries;
using OnlineShop.Modules.Inventory.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Modules.Inventory.Infrastructure.DependencyInjection;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("Postgres"),
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", InventoryDbContext.Schema)));

        services.AddScoped<IInventoryUnitOfWork>(sp => sp.GetRequiredService<InventoryDbContext>());

        services.AddScoped<IProductStockRepository, ProductStockRepository>();
        services.AddScoped<IInventoryQueries, InventoryQueries>();

        return services;
    }
}
