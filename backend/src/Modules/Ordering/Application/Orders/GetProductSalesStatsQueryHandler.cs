using MediatR;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed class GetProductSalesStatsQueryHandler : IRequestHandler<GetProductSalesStatsQuery, IReadOnlyList<ProductSalesStatsDto>>
{
    private readonly IOrderQueries _queries;

    public GetProductSalesStatsQueryHandler(IOrderQueries queries)
    {
        _queries = queries;
    }

    public Task<IReadOnlyList<ProductSalesStatsDto>> Handle(GetProductSalesStatsQuery request, CancellationToken cancellationToken) =>
        _queries.GetProductSalesStatsAsync(request.TopN, cancellationToken);
}
