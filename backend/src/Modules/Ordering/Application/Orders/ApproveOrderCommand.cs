using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed record ApproveOrderCommand(Guid OrderId, Guid ApprovedByUserId) : IRequest<Result>;
