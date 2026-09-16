using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.BuildingBlocks.Infrastructure;
using OnlineShop.Modules.AiAdvisory.Application.Abstractions;
using OnlineShop.Modules.AiAdvisory.Domain;

namespace OnlineShop.Modules.AiAdvisory.Infrastructure.Persistence;

public sealed class AiAdvisoryDbContext : AppDbContextBase, IAiAdvisoryUnitOfWork
{
    public const string Schema = "ai";

    public AiAdvisoryDbContext(DbContextOptions<AiAdvisoryDbContext> options, IPublisher publisher)
        : base(options, publisher)
    {
    }

    public DbSet<AiInteractionLog> AiInteractionLogs => Set<AiInteractionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiAdvisoryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
