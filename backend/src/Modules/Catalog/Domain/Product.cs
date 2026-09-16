using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Catalog.Domain.Events;
using OnlineShop.Modules.Catalog.Domain.ValueObjects;

namespace OnlineShop.Modules.Catalog.Domain;

/// <summary>
/// Owns display/catalog data only (name, description, price, image, category). Stock is
/// intentionally not here — it belongs to Inventory's ProductStock aggregate (Phase 3), kept
/// separate so editing a product's description never contends with a stock-issuing transaction.
/// StockQuantity below is a read-only *display cache*, kept in sync by Inventory publishing its
/// StockReceived/Issued/Returned domain events — Inventory.ProductStock.QuantityOnHand remains
/// the one source of truth. Never mutate StockQuantity from within Catalog itself.
/// </summary>
public sealed class Product : AggregateRoot<Guid>
{
    private readonly List<ProductImage> _images = new();

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? NameEn { get; private set; }
    public string? NameVi { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string? DescriptionVi { get; private set; }
    public Money Price { get; private set; } = default!;
    public Guid CategoryId { get; private set; }
    public string? ImageUrl { get; private set; }
    public int StockQuantity { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private Product() { }

    private Product(
        Guid id,
        string name,
        string? description,
        Money price,
        Guid categoryId,
        string? nameEn,
        string? nameVi,
        string? descriptionEn,
        string? descriptionVi) : base(id)
    {
        Name = name;
        Description = description;
        NameEn = nameEn;
        NameVi = nameVi;
        DescriptionEn = descriptionEn;
        DescriptionVi = descriptionVi;
        Price = price;
        CategoryId = categoryId;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static Product Create(
        string name,
        string? description,
        Money price,
        Guid categoryId,
        string? nameEn = null,
        string? nameVi = null,
        string? descriptionEn = null,
        string? descriptionVi = null)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        var product = new Product(
            Guid.NewGuid(),
            name.Trim(),
            NormalizeOptional(description),
            price,
            categoryId,
            NormalizeOptional(nameEn),
            NormalizeOptional(nameVi),
            NormalizeOptional(descriptionEn),
            NormalizeOptional(descriptionVi));
        product.Raise(new ProductCreatedDomainEvent(product.Id, product.Name));
        return product;
    }

    public void UpdateDetails(
        string name,
        string? description,
        Money price,
        Guid categoryId,
        string? nameEn = null,
        string? nameVi = null,
        string? descriptionEn = null,
        string? descriptionVi = null)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name.Trim();
        Description = NormalizeOptional(description);
        NameEn = NormalizeOptional(nameEn);
        NameVi = NormalizeOptional(nameVi);
        DescriptionEn = NormalizeOptional(descriptionEn);
        DescriptionVi = NormalizeOptional(descriptionVi);
        Price = price;
        CategoryId = categoryId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public void UpdateImage(string imageUrl)
    {
        Guard.AgainstNullOrWhiteSpace(imageUrl, nameof(imageUrl));
        ImageUrl = imageUrl;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Projection sync only — called from the handler reacting to Inventory's stock
    /// events, never from a user-facing command. Not a business rule of Catalog's own, so it
    /// doesn't raise a domain event or bump UpdatedAtUtc.</summary>
    public void SyncStockDisplay(int quantityOnHand) => StockQuantity = quantityOnHand;

    public void AddImage(string url)
    {
        Guard.AgainstNullOrWhiteSpace(url, nameof(url));
        var nextSortOrder = _images.Count == 0 ? 0 : _images.Max(i => i.SortOrder) + 1;
        _images.Add(ProductImage.Create(url, nextSortOrder));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.SingleOrDefault(i => i.Id == imageId);
        if (image is null)
            throw new DomainException("This image does not belong to this product.");

        _images.Remove(image);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Called right before the handler removes this aggregate — the row is going away
    /// either way, this only exists so Inventory gets a chance to clean up the now-orphaned
    /// ProductStock row. EF still dispatches domain events for entities in the Deleted state.</summary>
    public void Delete() => Raise(new ProductDeletedDomainEvent(Id));
}
