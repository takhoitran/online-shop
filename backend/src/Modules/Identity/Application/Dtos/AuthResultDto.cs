namespace OnlineShop.Modules.Identity.Application.Dtos;

public sealed record AuthResultDto(
    Guid UserId,
    string Username,
    string FullName,
    string Role,
    string Token,
    DateTime ExpiresAtUtc);
