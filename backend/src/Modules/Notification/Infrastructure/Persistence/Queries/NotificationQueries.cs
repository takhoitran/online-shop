using Microsoft.EntityFrameworkCore;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Notification.Application.Abstractions;
using OnlineShop.Modules.Notification.Application.Dtos;

namespace OnlineShop.Modules.Notification.Infrastructure.Persistence.Queries;

internal sealed class NotificationQueries : INotificationQueries
{
    private readonly NotificationDbContext _dbContext;

    public NotificationQueries(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<NotificationDto>> GetForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Notifications.AsNoTracking().Where(n => n.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var items = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(n => new NotificationDto(
                n.Id,
                n.Type.ToString(),
                n.Title,
                n.Message,
                n.RelatedOrderId,
                n.RelatedProductId,
                n.IsRead,
                n.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationDto>(items, safePage, safePageSize, totalCount);
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _dbContext.Notifications.AsNoTracking().CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
}
