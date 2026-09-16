using MediatR;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;
