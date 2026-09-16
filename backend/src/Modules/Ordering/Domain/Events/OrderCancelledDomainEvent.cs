using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain.Events;

/// <summary>WasApproved tells Inventory whether stock was ever issued for this order and
/// therefore needs to be returned (Cancel from Pending never touched stock).</summary>
public sealed record OrderCancelledDomainEvent(
    Guid OrderId,
    Guid CancelledByUserId,
    bool WasApproved,
    IReadOnlyList<OrderLineItem> Lines) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
