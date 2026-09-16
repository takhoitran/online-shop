using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Products.Include(p => p.Images).SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Add(Product product) => _dbContext.Products.Add(product);

    public void Remove(Product product) => _dbContext.Products.Remove(product);
}
