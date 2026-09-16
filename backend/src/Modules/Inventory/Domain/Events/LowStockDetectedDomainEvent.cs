using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Inventory.Domain.Events;

/// <summary>Consumed by Notification (Phase 9) to alert the Seller. Not handled yet — MediatR
/// publishing to zero handlers is a no-op, so raising it now is safe and forward-compatible.</summary>
public sealed record LowStockDetectedDomainEvent(Guid ProductId, int QuantityOnHand) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
