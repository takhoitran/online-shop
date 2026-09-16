using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain;

/// <summary>
/// One per Buyer, kept server-side (not browser localStorage) precisely so it's shared between
/// Web and Telegram — the same person adding an item from the bot must see it on the website too.
/// </summary>
public sealed class Cart : AggregateRoot<Guid>
{
    private readonly List<CartItem> _items = new();

    public Guid UserId { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    private Cart(Guid id, Guid userId) : base(id)
    {
        UserId = userId;
    }

    public static Cart CreateFor(Guid userId) => new(Guid.NewGuid(), userId);

    public void AddItem(Guid productId, int quantity)
    {
        var existing = _items.SingleOrDefault(i => i.ProductId == productId);
        if (existing is not null)
            existing.IncreaseQuantity(quantity);
        else
            _items.Add(CartItem.Create(productId, quantity));
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = _items.SingleOrDefault(i => i.ProductId == productId)
            ?? throw new DomainException("This product is not in the cart.");

        if (quantity <= 0)
            _items.Remove(item);
        else
            item.SetQuantity(quantity);
    }

    public void RemoveItem(Guid productId) => _items.RemoveAll(i => i.ProductId == productId);

    public void Clear() => _items.Clear();
}
