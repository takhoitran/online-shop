using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain.Events;

/// <summary>A completed (delivered) order being returned — Inventory returns stock for every line.</summary>
public sealed record OrderReturnedDomainEvent(
    Guid OrderId,
    Guid ProcessedByUserId,
    IReadOnlyList<OrderLineItem> Lines) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
