using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Application.Reviews;

public sealed class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, PagedResult<ProductReviewDto>>
{
    private readonly IProductReviewQueries _reviewQueries;

    public GetProductReviewsQueryHandler(IProductReviewQueries reviewQueries)
    {
        _reviewQueries = reviewQueries;
    }

    public Task<PagedResult<ProductReviewDto>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken) =>
        _reviewQueries.GetForProductAsync(request.ProductId, request.Page, request.PageSize, cancellationToken);
}
