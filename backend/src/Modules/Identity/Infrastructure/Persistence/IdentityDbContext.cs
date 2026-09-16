using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.BuildingBlocks.Infrastructure;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Domain;

namespace OnlineShop.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : AppDbContextBase, IIdentityUnitOfWork
{
    public const string Schema = "identity";

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, IPublisher publisher)
        : base(options, publisher)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<TelegramLinkCode> TelegramLinkCodes => Set<TelegramLinkCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
