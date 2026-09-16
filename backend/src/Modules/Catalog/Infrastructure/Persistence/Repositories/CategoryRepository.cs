using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _dbContext;

    public CategoryRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Categories.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Categories.OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Categories.AnyAsync(c => c.Id == id, cancellationToken);

    public void Add(Category category) => _dbContext.Categories.Add(category);

    public void Remove(Category category) => _dbContext.Categories.Remove(category);
}
