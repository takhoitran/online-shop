using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Ordering.Domain.ValueObjects;

namespace OnlineShop.Modules.Ordering.Domain;

/// <summary>
/// One line item, frozen at the moment of Checkout — ProductName and UnitPrice are snapshots, so
/// the order's history stays accurate even if the product is later renamed, repriced, or deleted
/// from Catalog. Only ever created inside Order.Place(); no public factory of its own.
/// </summary>
public sealed class OrderDetail : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = default!;

    public Money LineTotal => Money.Of(UnitPrice.Amount * Quantity, UnitPrice.Currency);

    private OrderDetail() { }

    private OrderDetail(Guid id, Guid productId, string productName, int quantity, Money unitPrice) : base(id)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal static OrderDetail Create(Guid productId, string productName, int quantity, Money unitPrice)
    {
        Guard.AgainstNullOrWhiteSpace(productName, nameof(productName));
        if (quantity <= 0)
            throw new DomainException("Item quantity in an order must be greater than 0.");

        return new OrderDetail(Guid.NewGuid(), productId, productName, quantity, unitPrice);
    }
}
