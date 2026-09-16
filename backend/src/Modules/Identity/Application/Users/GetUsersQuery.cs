using MediatR;
using OnlineShop.Modules.Identity.Application.Dtos;

namespace OnlineShop.Modules.Identity.Application.Users;

/// <summary>Admin-only — see AdminUsersController.</summary>
public sealed record GetUsersQuery : IRequest<List<UserSummaryDto>>;
