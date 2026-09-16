using OnlineShop.Modules.Identity.Domain;

namespace OnlineShop.Modules.Identity.Infrastructure.Security;

internal sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => BCrypt.Net.BCrypt.EnhancedHashPassword(plainPassword, workFactor: 12);

    public bool Verify(string plainPassword, string passwordHash) =>
        BCrypt.Net.BCrypt.EnhancedVerify(plainPassword, passwordHash);
}
