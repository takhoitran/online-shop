using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Orders;

/// <summary>IsStaff comes from the caller's role (Seller/Admin can cancel any order; a Buyer can
/// only cancel their own) — resolved in the Controller, not trusted from client input.</summary>
public sealed record CancelOrderCommand(Guid OrderId, Guid RequestedByUserId, bool IsStaff, string? Reason) : IRequest<Result>;
