namespace OnlineShop.Modules.Catalog.Domain;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Product product);
    void Remove(Product product);
}
