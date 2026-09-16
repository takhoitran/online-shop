using MediatR;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderQueries _queries;

    public GetOrderByIdQueryHandler(IOrderQueries queries)
    {
        _queries = queries;
    }

    public Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken) =>
        _queries.GetByIdAsync(request.OrderId, cancellationToken);
}
