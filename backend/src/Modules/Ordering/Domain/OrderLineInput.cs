using OnlineShop.Modules.Ordering.Domain.ValueObjects;

namespace OnlineShop.Modules.Ordering.Domain;

/// <summary>Input to Order.Place() — the Application layer fetches current name/price from
/// Catalog and hands them in already resolved; the Domain never reaches out to another module.</summary>
public sealed record OrderLineInput(Guid ProductId, string ProductName, int Quantity, Money UnitPrice);
