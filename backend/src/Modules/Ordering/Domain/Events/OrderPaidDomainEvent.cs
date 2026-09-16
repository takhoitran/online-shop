using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain.Events;

/// <summary>No handler yet — Notification (Phase 9) will use this to confirm payment to the buyer.</summary>
public sealed record OrderPaidDomainEvent(Guid OrderId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
