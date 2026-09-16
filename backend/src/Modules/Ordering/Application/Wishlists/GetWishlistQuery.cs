using MediatR;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Wishlists;

public sealed record GetWishlistQuery(Guid UserId) : IRequest<WishlistDto>;
