using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Carts;

/// <summary>Quantity 0 removes the line — matches typical "stepper down to zero" cart UX.</summary>
public sealed record UpdateCartItemQuantityCommand(Guid UserId, Guid ProductId, int Quantity) : IRequest<Result>;
