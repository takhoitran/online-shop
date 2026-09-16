using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Identity.Application.Dtos;

namespace OnlineShop.Modules.Identity.Application.Users;

/// <summary>
/// How Seller (and, rarely, another Admin) accounts come into existence — Admin-only, unlike
/// public self-registration which is Buyer-only (see RegisterUserCommand).
/// </summary>
public sealed record CreateStaffUserCommand(
    string Username,
    string Password,
    string FullName,
    string RoleName) : IRequest<Result<UserSummaryDto>>;
