using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Notification.Domain;

/// <summary>
/// One in-app notification row per recipient per event — the durable, always-available half of
/// delivery (Telegram push is best-effort on top of this, never a replacement for it: a buyer who
/// never linked Telegram, or whose push failed, still sees this in their in-app inbox).
/// </summary>
public sealed class AppNotification : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public Guid? RelatedOrderId { get; private set; }
    public Guid? RelatedProductId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AppNotification() { }

    private AppNotification(
        Guid id,
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid? relatedOrderId,
        Guid? relatedProductId) : base(id)
    {
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        RelatedOrderId = relatedOrderId;
        RelatedProductId = relatedProductId;
        IsRead = false;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static AppNotification Create(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid? relatedOrderId = null,
        Guid? relatedProductId = null)
    {
        Guard.AgainstNullOrWhiteSpace(title, nameof(title));
        Guard.AgainstNullOrWhiteSpace(message, nameof(message));

        return new AppNotification(Guid.NewGuid(), userId, type, title, message, relatedOrderId, relatedProductId);
    }

    public void MarkAsRead() => IsRead = true;
}
