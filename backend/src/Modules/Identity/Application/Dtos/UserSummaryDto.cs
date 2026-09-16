namespace OnlineShop.Modules.Identity.Application.Dtos;

public sealed record UserSummaryDto(
    Guid Id,
    string Username,
    string FullName,
    string Role,
    bool HasTelegramLinked,
    DateTime CreatedAtUtc);
