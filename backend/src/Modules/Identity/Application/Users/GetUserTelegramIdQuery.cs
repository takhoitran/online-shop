using MediatR;

namespace OnlineShop.Modules.Identity.Application.Users;

/// <summary>Internal cross-module lookup — used by Notification (Phase 9) to decide whether a
/// buyer/staff member can be reached on Telegram, not exposed via any Api controller.</summary>
public sealed record GetUserTelegramIdQuery(Guid UserId) : IRequest<string?>;
