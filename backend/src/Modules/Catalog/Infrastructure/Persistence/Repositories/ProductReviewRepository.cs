using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class ProductReviewRepository : IProductReviewRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductReviewRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductReview?> GetByProductAndBuyerAsync(Guid productId, Guid buyerId, CancellationToken cancellationToken = default) =>
        _dbContext.ProductReviews.SingleOrDefaultAsync(r => r.ProductId == productId && r.BuyerId == buyerId, cancellationToken);

    public Task<ProductReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ProductReviews.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    public void Add(ProductReview review) => _dbContext.ProductReviews.Add(review);

    public void Remove(ProductReview review) => _dbContext.ProductReviews.Remove(review);
}
