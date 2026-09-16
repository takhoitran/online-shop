namespace OnlineShop.Modules.Inventory.Domain;

public interface IProductStockRepository
{
    Task<ProductStock?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForProductAsync(Guid productId, CancellationToken cancellationToken = default);
    void Add(ProductStock stock);
    void Remove(ProductStock stock);
}
