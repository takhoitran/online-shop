namespace OnlineShop.Modules.Notification.Application.Dtos;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    Guid? RelatedOrderId,
    Guid? RelatedProductId,
    bool IsRead,
    DateTime CreatedAtUtc);
