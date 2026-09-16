using MediatR;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Carts;

public sealed record GetCartQuery(Guid UserId) : IRequest<CartDto>;
