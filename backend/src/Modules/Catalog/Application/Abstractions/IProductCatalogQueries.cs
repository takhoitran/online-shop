using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Application.Abstractions;

/// <summary>SortBy: "newest" (default), "price_asc", "price_desc", or "name_asc".</summary>
public sealed record ProductSearchFilter(
    string? Keyword,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    double? MinRating = null);

/// <summary>
/// Read-side port: goes straight at the persistence store for cheap projections instead of
/// loading/rehydrating full Aggregates just to map them to a DTO. Implemented in Infrastructure
/// directly against CatalogDbContext. This is also the "Open Host Service" other modules
/// (Ordering, AiAdvisory — see the Context Map) call into for product info.
/// </summary>
public interface IProductCatalogQueries
{
    Task<PagedResult<ProductListItemDto>> SearchAsync(ProductSearchFilter filter, CancellationToken cancellationToken = default);
    Task<ProductDetailDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
