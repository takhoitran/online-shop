using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain;

public sealed class WishlistItem : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public DateTime AddedAtUtc { get; private set; }

    private WishlistItem() { }

    private WishlistItem(Guid id, Guid productId) : base(id)
    {
        ProductId = productId;
        AddedAtUtc = DateTime.UtcNow;
    }

    internal static WishlistItem Create(Guid productId) => new(Guid.NewGuid(), productId);
}
