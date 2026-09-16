using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed record MarkOrderPaidCommand(Guid OrderId) : IRequest<Result>;
