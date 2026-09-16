using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Orders;

/// <summary>UserId null + caller is staff -> see all orders. Controller enforces a Buyer can only
/// ever pass their own UserId.</summary>
public sealed record GetOrdersQuery(Guid? UserId, string? Status, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<OrderSummaryDto>>;
