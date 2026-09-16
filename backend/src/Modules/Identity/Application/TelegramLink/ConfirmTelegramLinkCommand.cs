using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Identity.Application.TelegramLink;

/// <summary>Called from the Telegram Channel Adapter when the buyer/seller sends the code to the bot.</summary>
public sealed record ConfirmTelegramLinkCommand(string Code, string TelegramId) : IRequest<Result>;
