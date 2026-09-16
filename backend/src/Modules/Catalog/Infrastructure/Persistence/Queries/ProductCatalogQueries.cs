using Microsoft.EntityFrameworkCore;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence.Queries;

internal sealed class ProductCatalogQueries : IProductCatalogQueries
{
    private readonly CatalogDbContext _dbContext;

    public ProductCatalogQueries(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ProductListItemDto>> SearchAsync(
        ProductSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query =
            from product in _dbContext.Products.AsNoTracking()
            join category in _dbContext.Categories.AsNoTracking() on product.CategoryId equals category.Id
            select new
            {
                product,
                category,
                AverageRating = _dbContext.ProductReviews.Where(r => r.ProductId == product.Id).Average(r => (double?)r.Rating),
                ReviewCount = _dbContext.ProductReviews.Count(r => r.ProductId == product.Id),
            };

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            // unaccent() on both sides — inside the LINQ expression, translated to SQL by EF —
            // so a correctly-accented search ("áo") matches a name stored without diacritics
            // ("Ao thun") and vice versa. Calling CatalogDbContext.Unaccent outside a query
            // expression would just hit its NotSupportedException body; it only works because
            // `filter.Keyword` here is captured as part of the expression tree, not evaluated
            // eagerly in .NET.
            var keyword = filter.Keyword;
            query = query.Where(x =>
                EF.Functions.ILike(CatalogDbContext.Unaccent(x.product.Name), "%" + CatalogDbContext.Unaccent(keyword) + "%") ||
                (x.product.NameEn != null && EF.Functions.ILike(CatalogDbContext.Unaccent(x.product.NameEn), "%" + CatalogDbContext.Unaccent(keyword) + "%")) ||
                (x.product.NameVi != null && EF.Functions.ILike(CatalogDbContext.Unaccent(x.product.NameVi), "%" + CatalogDbContext.Unaccent(keyword) + "%")));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(x => x.product.CategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(x => x.product.Price.Amount >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(x => x.product.Price.Amount <= filter.MaxPrice.Value);

        if (filter.MinRating.HasValue)
            query = query.Where(x => x.AverageRating != null && x.AverageRating >= filter.MinRating.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        query = filter.SortBy switch
        {
            "price_asc" => query.OrderBy(x => x.product.Price.Amount),
            "price_desc" => query.OrderByDescending(x => x.product.Price.Amount),
            "name_asc" => query.OrderBy(x => x.product.Name),
            _ => query.OrderByDescending(x => x.product.CreatedAtUtc),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductListItemDto(
                x.product.Id,
                x.product.Name,
                x.product.NameEn,
                x.product.NameVi,
                x.product.Price.Amount,
                x.product.Price.Currency,
                x.product.ImageUrl,
                x.product.StockQuantity,
                x.category.Id,
                x.category.Name,
                x.category.NameEn,
                x.category.NameVi,
                x.AverageRating,
                x.ReviewCount))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<ProductDetailDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await (
            from product in _dbContext.Products.AsNoTracking()
            join category in _dbContext.Categories.AsNoTracking() on product.CategoryId equals category.Id
            where product.Id == productId
            select new ProductDetailDto(
                product.Id,
                product.Name,
                product.Description,
                product.NameEn,
                product.NameVi,
                product.DescriptionEn,
                product.DescriptionVi,
                product.Price.Amount,
                product.Price.Currency,
                product.ImageUrl,
                product.StockQuantity,
                category.Id,
                category.Name,
                category.NameEn,
                category.NameVi,
                product.CreatedAtUtc,
                product.UpdatedAtUtc,
                _dbContext.ProductReviews.Where(r => r.ProductId == product.Id).Average(r => (double?)r.Rating),
                _dbContext.ProductReviews.Count(r => r.ProductId == product.Id),
                product.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto(i.Id, i.Url, i.SortOrder)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
