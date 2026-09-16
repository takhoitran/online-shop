using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Inventory.Domain;

namespace OnlineShop.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class ProductStockRepository : IProductStockRepository
{
    private readonly InventoryDbContext _dbContext;

    public ProductStockRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductStock?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
        _dbContext.ProductStocks
            .Include(s => s.Transactions)
            .SingleOrDefaultAsync(s => s.Id == productId, cancellationToken);

    public Task<bool> ExistsForProductAsync(Guid productId, CancellationToken cancellationToken = default) =>
        _dbContext.ProductStocks.AnyAsync(s => s.Id == productId, cancellationToken);

    public void Add(ProductStock stock) => _dbContext.ProductStocks.Add(stock);

    public void Remove(ProductStock stock) => _dbContext.ProductStocks.Remove(stock);
}
