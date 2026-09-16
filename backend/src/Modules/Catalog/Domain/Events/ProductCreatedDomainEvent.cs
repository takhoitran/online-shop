using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Catalog.Domain.Events;

/// <summary>Inventory (Phase 3) will listen for this to initialize a ProductStock row at 0 —
/// Catalog never creates stock rows itself, it just announces a sellable product now exists.</summary>
public sealed record ProductCreatedDomainEvent(Guid ProductId, string ProductName) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
