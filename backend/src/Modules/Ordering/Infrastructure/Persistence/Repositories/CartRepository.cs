using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Repositories;

internal sealed class CartRepository : ICartRepository
{
    private readonly OrderingDbContext _dbContext;

    public CartRepository(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _dbContext.Carts.Include(c => c.Items).SingleOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public void Add(Cart cart) => _dbContext.Carts.Add(cart);
}
