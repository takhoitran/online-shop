using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Repositories;

internal sealed class WishlistRepository : IWishlistRepository
{
    private readonly OrderingDbContext _dbContext;

    public WishlistRepository(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _dbContext.Wishlists.Include(w => w.Items).SingleOrDefaultAsync(w => w.UserId == userId, cancellationToken);

    public void Add(Wishlist wishlist) => _dbContext.Wishlists.Add(wishlist);
}
