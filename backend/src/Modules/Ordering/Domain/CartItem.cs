using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain;

/// <summary>No price snapshot here — a cart shows live prices; only Order.Place() freezes one.</summary>
public sealed class CartItem : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    private CartItem() { }

    private CartItem(Guid id, Guid productId, int quantity) : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    internal static CartItem Create(Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than 0.");

        return new CartItem(Guid.NewGuid(), productId, quantity);
    }

    internal void IncreaseQuantity(int by) => Quantity += by;

    internal void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than 0.");
        Quantity = quantity;
    }
}
