using Microsoft.EntityFrameworkCore;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Infrastructure.Persistence.Queries;

internal sealed class ProductReviewQueries : IProductReviewQueries
{
    private readonly CatalogDbContext _dbContext;

    public ProductReviewQueries(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ProductReviewDto>> GetForProductAsync(
        Guid productId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ProductReviews.AsNoTracking().Where(r => r.ProductId == productId);

        var totalCount = await query.CountAsync(cancellationToken);

        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(r => new ProductReviewDto(r.Id, r.ProductId, r.BuyerId, r.BuyerName, r.Rating, r.Comment, r.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductReviewDto>(items, safePage, safePageSize, totalCount);
    }
}
