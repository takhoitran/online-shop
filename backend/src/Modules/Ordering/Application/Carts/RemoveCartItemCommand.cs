using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Carts;

public sealed record RemoveCartItemCommand(Guid UserId, Guid ProductId) : IRequest<Result>;
