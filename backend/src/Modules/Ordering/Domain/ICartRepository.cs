namespace OnlineShop.Modules.Ordering.Domain;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(Cart cart);
}
