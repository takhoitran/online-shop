using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Identity.Domain;

/// <summary>
/// Short-lived one-time code a logged-in Web user generates and then sends to the Telegram
/// bot to prove they own both accounts. Kept as its own small Aggregate (Vernon, ch. 10:
/// design small aggregates) because its lifecycle — generate, consume once, expire — has
/// nothing to do with the rest of the User aggregate's lifecycle.
/// </summary>
public sealed class TelegramLinkCode : AggregateRoot<Guid>
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);

    public Guid UserId { get; private set; }
    public string Code { get; private set; } = default!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? ConsumedAtUtc { get; private set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAtUtc;
    public bool IsConsumed => ConsumedAtUtc is not null;

    private TelegramLinkCode() { }

    private TelegramLinkCode(Guid id, Guid userId, string code) : base(id)
    {
        UserId = userId;
        Code = code;
        ExpiresAtUtc = DateTime.UtcNow.Add(Ttl);
    }

    public static TelegramLinkCode Generate(Guid userId)
    {
        var code = Random.Shared.Next(0, 1_000_000).ToString("D6");
        return new TelegramLinkCode(Guid.NewGuid(), userId, code);
    }

    public void Consume()
    {
        if (IsConsumed)
            throw new DomainException("This link code has already been used.");

        if (IsExpired)
            throw new DomainException("This link code has expired, please generate a new one.");

        ConsumedAtUtc = DateTime.UtcNow;
    }
}
