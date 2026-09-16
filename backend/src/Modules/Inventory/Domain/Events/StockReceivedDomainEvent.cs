using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Inventory.Domain.Events;

public sealed record StockReceivedDomainEvent(Guid ProductId, int QuantityOnHand) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
