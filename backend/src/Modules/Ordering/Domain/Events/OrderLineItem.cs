namespace OnlineShop.Modules.Ordering.Domain.Events;

/// <summary>Plain data carried on order events — Inventory reacts to these without ever loading
/// an Order aggregate (it isn't Inventory's to load).</summary>
public sealed record OrderLineItem(Guid ProductId, int Quantity);
