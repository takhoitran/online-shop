using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Identity.Application.Dtos;

namespace OnlineShop.Modules.Identity.Application.Register;

/// <summary>Public self-registration is Buyer-only. Seller/Admin accounts are provisioned by an
/// Admin — see Identity.Application.Users.CreateStaffUserCommand.</summary>
public sealed record RegisterUserCommand(
    string Username,
    string Password,
    string FullName) : IRequest<Result<AuthResultDto>>;
