using MediatR;
using OnlineShop.Modules.Inventory.Application.Abstractions;
using OnlineShop.Modules.Inventory.Application.Dtos;

namespace OnlineShop.Modules.Inventory.Application.GetProductStock;

public sealed class GetProductStockQueryHandler : IRequestHandler<GetProductStockQuery, ProductStockDto?>
{
    private readonly IInventoryQueries _queries;

    public GetProductStockQueryHandler(IInventoryQueries queries)
    {
        _queries = queries;
    }

    public Task<ProductStockDto?> Handle(GetProductStockQuery request, CancellationToken cancellationToken) =>
        _queries.GetByProductIdAsync(request.ProductId, cancellationToken);
}
