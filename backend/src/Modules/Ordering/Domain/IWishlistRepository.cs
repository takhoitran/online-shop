namespace OnlineShop.Modules.Ordering.Domain;

public interface IWishlistRepository
{
    Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(Wishlist wishlist);
}
