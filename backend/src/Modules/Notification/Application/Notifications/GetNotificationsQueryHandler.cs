using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Notification.Application.Abstractions;
using OnlineShop.Modules.Notification.Application.Dtos;

namespace OnlineShop.Modules.Notification.Application.Notifications;

public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly INotificationQueries _queries;

    public GetNotificationsQueryHandler(INotificationQueries queries)
    {
        _queries = queries;
    }

    public Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken) =>
        _queries.GetForUserAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
}
