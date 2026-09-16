using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Notification.Application.Notifications;

public sealed record MarkNotificationAsReadCommand(Guid UserId, Guid NotificationId) : IRequest<Result>;
