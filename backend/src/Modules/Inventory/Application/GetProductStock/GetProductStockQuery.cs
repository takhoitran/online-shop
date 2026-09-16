using MediatR;
using OnlineShop.Modules.Inventory.Application.Dtos;

namespace OnlineShop.Modules.Inventory.Application.GetProductStock;

public sealed record GetProductStockQuery(Guid ProductId) : IRequest<ProductStockDto?>;
