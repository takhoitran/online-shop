using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IOrderQueries _queries;

    public GetOrdersQueryHandler(IOrderQueries queries)
    {
        _queries = queries;
    }

    public Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken) =>
        _queries.SearchAsync(new OrderSearchFilter(request.UserId, request.Status, request.Page, request.PageSize), cancellationToken);
}
