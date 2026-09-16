using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed record CompleteOrderCommand(Guid OrderId) : IRequest<Result>;
