using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Inventory.Domain.Events;

public sealed record StockIssuedDomainEvent(Guid ProductId, int QuantityOnHand, Guid? OrderId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
