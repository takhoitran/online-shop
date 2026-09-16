using OnlineShop.Modules.Notification.Domain;

namespace OnlineShop.Modules.Notification.Application.Abstractions;

public interface IAppNotificationRepository
{
    void Add(AppNotification notification);

    /// <summary>Scoped to userId as well as id, so a user can never mark someone else's notification read.</summary>
    Task<AppNotification?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
