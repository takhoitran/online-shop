using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Catalog.Application.Products;

/// <summary>
/// Called only by Inventory's domain-event handlers (Inventory.Application references this
/// Application project — never the reverse) to keep Product.StockQuantity in sync after a
/// receive/issue/return. Not meant to be exposed on any controller.
/// </summary>
public sealed record UpdateProductStockDisplayCommand(Guid ProductId, int QuantityOnHand) : IRequest<Result>;
