using MediatR;
using OnlineShop.Modules.Identity.Application.Dtos;

namespace OnlineShop.Modules.Identity.Application.Users;

public sealed record GetUserByTelegramIdQuery(string TelegramId) : IRequest<UserSummaryDto?>;
