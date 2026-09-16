using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain;

/// <summary>
/// One per Buyer, same shape as Cart — server-side so it's shared between Web and Telegram.
/// Unlike Cart, adding an already-present product is just a no-op (toggling a heart icon twice
/// should never be an error the buyer has to think about).
/// </summary>
public sealed class Wishlist : AggregateRoot<Guid>
{
    private readonly List<WishlistItem> _items = new();

    public Guid UserId { get; private set; }
    public IReadOnlyCollection<WishlistItem> Items => _items.AsReadOnly();

    private Wishlist() { }

    private Wishlist(Guid id, Guid userId) : base(id)
    {
        UserId = userId;
    }

    public static Wishlist CreateFor(Guid userId) => new(Guid.NewGuid(), userId);

    public void AddItem(Guid productId)
    {
        if (_items.Any(i => i.ProductId == productId))
            return;

        _items.Add(WishlistItem.Create(productId));
    }

    public void RemoveItem(Guid productId) => _items.RemoveAll(i => i.ProductId == productId);
}
