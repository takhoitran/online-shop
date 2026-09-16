namespace OnlineShop.Modules.Catalog.Domain;

public interface IProductReviewRepository
{
    Task<ProductReview?> GetByProductAndBuyerAsync(Guid productId, Guid buyerId, CancellationToken cancellationToken = default);
    Task<ProductReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(ProductReview review);
    void Remove(ProductReview review);
}
