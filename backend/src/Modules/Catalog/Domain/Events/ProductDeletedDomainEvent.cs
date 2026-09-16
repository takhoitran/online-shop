using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Catalog.Domain.Events;

/// <summary>Inventory listens for this to remove the now-orphaned ProductStock row (and its
/// transaction history, cascading) — mirrors ProductCreatedDomainEvent's "Inventory reacts to
/// Catalog's product lifecycle" pattern in the other direction.</summary>
public sealed record ProductDeletedDomainEvent(Guid ProductId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
