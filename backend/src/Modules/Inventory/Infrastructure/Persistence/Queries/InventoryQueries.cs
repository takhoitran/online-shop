using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Inventory.Application.Abstractions;
using OnlineShop.Modules.Inventory.Application.Dtos;

namespace OnlineShop.Modules.Inventory.Infrastructure.Persistence.Queries;

internal sealed class InventoryQueries : IInventoryQueries
{
    private const int RecentTransactionsLimit = 20;

    private readonly InventoryDbContext _dbContext;

    public InventoryQueries(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductStockDto?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var stock = await _dbContext.ProductStocks
            .AsNoTracking()
            .Include(s => s.Transactions)
            .SingleOrDefaultAsync(s => s.Id == productId, cancellationToken);

        if (stock is null)
            return null;

        var recentTransactions = stock.Transactions
            .OrderByDescending(t => t.OccurredAtUtc)
            .Take(RecentTransactionsLimit)
            .Select(t => new InventoryTransactionDto(
                t.Id, t.Type.ToString(), t.Quantity, t.OrderId, t.PerformedByUserId, t.Note, t.OccurredAtUtc))
            .ToList();

        return new ProductStockDto(stock.ProductId, stock.QuantityOnHand, recentTransactions);
    }
}
