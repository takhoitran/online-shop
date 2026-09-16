using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.BuildingBlocks.Infrastructure;

/// <summary>
/// Base DbContext for every module. SaveChangesAsync persists first, then publishes each
/// tracked Aggregate's collected domain events through MediatR before returning — still inside
/// the same logical unit of work, since we're a Modular Monolith on one database.
/// </summary>
public abstract class AppDbContextBase : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    protected AppDbContextBase(DbContextOptions options, IPublisher publisher) : base(options)
    {
        _publisher = publisher;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = ChangeTracker.Entries()
            .Select(entry => entry.Entity)
            .OfType<IHasDomainEvents>()
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in aggregatesWithEvents)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();

            foreach (var domainEvent in events)
                await _publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
