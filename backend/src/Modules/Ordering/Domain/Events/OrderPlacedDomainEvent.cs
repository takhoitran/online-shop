using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain.Events;

public sealed record OrderPlacedDomainEvent(Guid OrderId, Guid BuyerUserId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
