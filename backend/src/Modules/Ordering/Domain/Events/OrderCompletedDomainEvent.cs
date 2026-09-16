using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain.Events;

public sealed record OrderCompletedDomainEvent(Guid OrderId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
