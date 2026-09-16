namespace OnlineShop.Modules.Identity.Domain;

/// <summary>
/// Hashing algorithm is an infrastructure concern (BCrypt), but the *need* to hash before
/// a password ever reaches the Domain is a business rule — so the interface lives here and
/// the implementation lives in Identity.Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string plainPassword);
    bool Verify(string plainPassword, string passwordHash);
}
