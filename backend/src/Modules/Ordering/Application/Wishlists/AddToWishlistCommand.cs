using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Wishlists;

public sealed record AddToWishlistCommand(Guid UserId, Guid ProductId) : IRequest<Result>;
