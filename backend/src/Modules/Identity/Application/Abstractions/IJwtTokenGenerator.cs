using OnlineShop.Modules.Identity.Domain;

namespace OnlineShop.Modules.Identity.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user, string roleName);
}
