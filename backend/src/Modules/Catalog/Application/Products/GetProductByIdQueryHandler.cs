using MediatR;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
{
    private readonly IProductCatalogQueries _queries;

    public GetProductByIdQueryHandler(IProductCatalogQueries queries)
    {
        _queries = queries;
    }

    public Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        _queries.GetByIdAsync(request.ProductId, cancellationToken);
}
