using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.UnitTests.Ordering;

public class WishlistTests
{
    [Fact]
    public void AddItem_AddsANewItem()
    {
        var wishlist = Wishlist.CreateFor(Guid.NewGuid());
        var productId = Guid.NewGuid();

        wishlist.AddItem(productId);

        Assert.Single(wishlist.Items);
        Assert.Equal(productId, wishlist.Items.Single().ProductId);
    }

    [Fact]
    public void AddItem_SameProductTwice_IsANoOp()
    {
        var wishlist = Wishlist.CreateFor(Guid.NewGuid());
        var productId = Guid.NewGuid();

        wishlist.AddItem(productId);
        wishlist.AddItem(productId);

        Assert.Single(wishlist.Items);
    }

    [Fact]
    public void RemoveItem_RemovesTheMatchingItem()
    {
        var wishlist = Wishlist.CreateFor(Guid.NewGuid());
        var productId = Guid.NewGuid();
        wishlist.AddItem(productId);

        wishlist.RemoveItem(productId);

        Assert.Empty(wishlist.Items);
    }

    [Fact]
    public void RemoveItem_WhenNotPresent_IsANoOp()
    {
        var wishlist = Wishlist.CreateFor(Guid.NewGuid());

        wishlist.RemoveItem(Guid.NewGuid());

        Assert.Empty(wishlist.Items);
    }
}
