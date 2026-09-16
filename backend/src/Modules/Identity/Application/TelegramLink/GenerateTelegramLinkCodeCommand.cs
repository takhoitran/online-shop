using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Identity.Application.Dtos;

namespace OnlineShop.Modules.Identity.Application.TelegramLink;

/// <summary>UserId comes from the authenticated JWT principal, not from client input.</summary>
public sealed record GenerateTelegramLinkCodeCommand(Guid UserId) : IRequest<Result<TelegramLinkCodeDto>>;
