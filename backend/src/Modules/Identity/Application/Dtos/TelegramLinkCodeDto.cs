namespace OnlineShop.Modules.Identity.Application.Dtos;

public sealed record TelegramLinkCodeDto(string Code, DateTime ExpiresAtUtc);
