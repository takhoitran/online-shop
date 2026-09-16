using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    private readonly IProductCatalogQueries _queries;

    public GetProductsQueryHandler(IProductCatalogQueries queries)
    {
        _queries = queries;
    }

    public Task<PagedResult<ProductListItemDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var filter = new ProductSearchFilter(
            request.Keyword,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.MinRating);

        return _queries.SearchAsync(filter, cancellationToken);
    }
}
