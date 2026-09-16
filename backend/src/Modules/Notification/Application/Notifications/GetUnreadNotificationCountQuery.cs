using MediatR;

namespace OnlineShop.Modules.Notification.Application.Notifications;

public sealed record GetUnreadNotificationCountQuery(Guid UserId) : IRequest<int>;
