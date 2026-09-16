using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain.Events;

/// <summary>Inventory reacts to this by issuing stock for every line (ProductStock.IssueStock)
/// — the "Ordering --domain event--> Inventory" edge from the Context Map.</summary>
public sealed record OrderApprovedDomainEvent(
    Guid OrderId,
    Guid ApprovedByUserId,
    IReadOnlyList<OrderLineItem> Lines) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
