using MediatR;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDetailDto?>;
