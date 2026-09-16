using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;
using OnlineShop.Modules.Ordering.Infrastructure.Persistence;
using OnlineShop.Modules.Ordering.Infrastructure.Persistence.Queries;
using OnlineShop.Modules.Ordering.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Modules.Ordering.Infrastructure.DependencyInjection;

public static class OrderingModule
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderingDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("Postgres"),
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", OrderingDbContext.Schema)));

        services.AddScoped<IOrderingUnitOfWork>(sp => sp.GetRequiredService<OrderingDbContext>());

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderQueries, OrderQueries>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<IVoucherRepository, VoucherRepository>();

        return services;
    }
}
