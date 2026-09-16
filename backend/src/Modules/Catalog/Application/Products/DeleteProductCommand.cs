using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed record DeleteProductCommand(Guid ProductId) : IRequest<Result>;
