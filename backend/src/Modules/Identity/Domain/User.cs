using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Identity.Domain.Events;
using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.Modules.Identity.Domain;

public sealed class User : AggregateRoot<Guid>
{
    public Username Username { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public Guid RoleId { get; private set; }
    public string? TelegramId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private User() { }

    private User(Guid id, Username username, string passwordHash, string fullName, Guid roleId)
        : base(id)
    {
        Username = username;
        PasswordHash = passwordHash;
        FullName = fullName;
        RoleId = roleId;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    /// <summary>
    /// passwordHash must already be hashed (IPasswordHasher.Hash) — the Aggregate never sees
    /// or stores a plaintext password.
    /// </summary>
    public static User Register(Username username, string passwordHash, string fullName, Guid roleId)
    {
        Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        Guard.AgainstNullOrWhiteSpace(fullName, nameof(fullName));

        var user = new User(Guid.NewGuid(), username, passwordHash, fullName.Trim(), roleId);
        user.Raise(new UserRegisteredDomainEvent(user.Id, username.Value, roleId));
        return user;
    }

    public void LinkTelegramAccount(string telegramId)
    {
        Guard.AgainstNullOrWhiteSpace(telegramId, nameof(telegramId));

        TelegramId = telegramId;
        UpdatedAtUtc = DateTime.UtcNow;
        Raise(new TelegramAccountLinkedDomainEvent(Id, telegramId));
    }

    public void ChangePassword(string newPasswordHash)
    {
        Guard.AgainstNullOrWhiteSpace(newPasswordHash, nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
