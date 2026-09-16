using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.BuildingBlocks.Infrastructure;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContext : AppDbContextBase, IOrderingUnitOfWork
{
    public const string Schema = "ordering";

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options, IPublisher publisher)
        : base(options, publisher)
    {
    }

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
