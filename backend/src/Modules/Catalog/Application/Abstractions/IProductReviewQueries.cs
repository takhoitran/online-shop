using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Application.Abstractions;

public interface IProductReviewQueries
{
    Task<PagedResult<ProductReviewDto>> GetForProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken = default);
}
