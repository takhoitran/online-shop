using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository : IOrderRepository
{
    private readonly OrderingDbContext _dbContext;

    public OrderRepository(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Orders.Include(o => o.Details).SingleOrDefaultAsync(o => o.Id == id, cancellationToken);

    public void Add(Order order) => _dbContext.Orders.Add(order);
}
