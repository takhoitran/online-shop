using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Carts;

public sealed record AddCartItemCommand(Guid UserId, Guid ProductId, int Quantity) : IRequest<Result>;
