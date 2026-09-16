using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Notification.Application.Dtos;

namespace OnlineShop.Modules.Notification.Application.Notifications;

public sealed record GetNotificationsQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<NotificationDto>>;
