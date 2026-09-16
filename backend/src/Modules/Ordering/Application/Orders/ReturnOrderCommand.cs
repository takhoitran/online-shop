using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed record ReturnOrderCommand(Guid OrderId, Guid ProcessedByUserId, string? Reason) : IRequest<Result>;
