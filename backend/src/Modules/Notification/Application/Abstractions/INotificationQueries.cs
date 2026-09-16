using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Notification.Application.Dtos;

namespace OnlineShop.Modules.Notification.Application.Abstractions;

public interface INotificationQueries
{
    Task<PagedResult<NotificationDto>> GetForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
