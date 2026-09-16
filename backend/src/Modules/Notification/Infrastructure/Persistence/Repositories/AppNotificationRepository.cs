using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Notification.Application.Abstractions;
using OnlineShop.Modules.Notification.Domain;

namespace OnlineShop.Modules.Notification.Infrastructure.Persistence.Repositories;

internal sealed class AppNotificationRepository : IAppNotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public AppNotificationRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(AppNotification notification) => _dbContext.Notifications.Add(notification);

    public Task<AppNotification?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
        _dbContext.Notifications.SingleOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);
}
