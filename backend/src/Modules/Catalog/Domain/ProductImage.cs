using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Catalog.Domain;

/// <summary>
/// A gallery image alongside Product's existing primary <c>ImageUrl</c> (the cover photo shown on
/// cards/thumbnails, unchanged) — extra angles/views shown on the product detail page. Child
/// entity of Product, not its own Aggregate: a product's gallery has no lifecycle or invariant
/// independent of the product itself.
/// </summary>
public sealed class ProductImage : Entity<Guid>
{
    public string Url { get; private set; } = default!;
    public int SortOrder { get; private set; }

    private ProductImage() { }

    private ProductImage(Guid id, string url, int sortOrder) : base(id)
    {
        Url = url;
        SortOrder = sortOrder;
    }

    internal static ProductImage Create(string url, int sortOrder)
    {
        Guard.AgainstNullOrWhiteSpace(url, nameof(url));
        return new ProductImage(Guid.NewGuid(), url, sortOrder);
    }
}
