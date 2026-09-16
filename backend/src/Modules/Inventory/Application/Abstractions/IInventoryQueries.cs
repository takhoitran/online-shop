using OnlineShop.Modules.Inventory.Application.Dtos;

namespace OnlineShop.Modules.Inventory.Application.Abstractions;

public interface IInventoryQueries
{
    Task<ProductStockDto?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
