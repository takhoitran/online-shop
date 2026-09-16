using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Catalog.Domain;

/// <summary>
/// Its own Aggregate Root, not a child entity of Product — a Product should never have to load
/// every review just to change its price, and two buyers reviewing the same product concurrently
/// must not contend with each other. BuyerName is a snapshot (same idea as OrderDetail.ProductName)
/// so Catalog never needs a cross-module call to Identity just to render a review.
/// </summary>
public sealed class ProductReview : AggregateRoot<Guid>
{
    public Guid ProductId { get; private set; }
    public Guid BuyerId { get; private set; }
    public string BuyerName { get; private set; } = default!;
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ProductReview() { }

    private ProductReview(Guid id, Guid productId, Guid buyerId, string buyerName, int rating, string? comment) : base(id)
    {
        ProductId = productId;
        BuyerId = buyerId;
        BuyerName = buyerName;
        Rating = rating;
        Comment = comment;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static ProductReview Create(Guid productId, Guid buyerId, string buyerName, int rating, string? comment)
    {
        Guard.AgainstNullOrWhiteSpace(buyerName, nameof(buyerName));
        if (rating is < 1 or > 5)
            throw new DomainException("Rating must be between 1 and 5.");

        return new ProductReview(Guid.NewGuid(), productId, buyerId, buyerName.Trim(), rating, comment?.Trim());
    }
}
