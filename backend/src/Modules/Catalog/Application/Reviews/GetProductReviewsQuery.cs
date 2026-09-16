using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Application.Reviews;

public sealed record GetProductReviewsQuery(Guid ProductId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ProductReviewDto>>;
