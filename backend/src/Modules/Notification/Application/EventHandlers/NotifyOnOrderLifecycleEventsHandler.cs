using MediatR;
using Microsoft.Extensions.Logging;
using OnlineShop.Modules.Notification.Application.Abstractions;
using OnlineShop.Modules.Notification.Domain;
using OnlineShop.Modules.Identity.Application.Users;
using OnlineShop.Modules.Ordering.Application.Orders;
using OnlineShop.Modules.Ordering.Domain.Events;

namespace OnlineShop.Modules.Notification.Application.EventHandlers;

/// <summary>
/// Reacts to every Order status-change event by notifying the buyer — always as an in-app row
/// (durable, always available), plus a best-effort Telegram push if they've linked their account
/// (see the plan's Phase 9 note: "gửi qua Telegram nếu đã link, fallback in-app nếu chưa").
/// None of these events except OrderPlaced carry the buyer's UserId directly (Approved/Cancelled/
/// Returned carry the *actor* — who could be a Seller, not the buyer), so every handler resolves
/// the real buyer via GetOrderByIdQuery rather than assuming the event's actor id is the recipient.
/// </summary>
public sealed class NotifyOnOrderLifecycleEventsHandler :
    INotificationHandler<OrderApprovedDomainEvent>,
    INotificationHandler<OrderCancelledDomainEvent>,
    INotificationHandler<OrderShippedDomainEvent>,
    INotificationHandler<OrderCompletedDomainEvent>,
    INotificationHandler<OrderReturnedDomainEvent>,
    INotificationHandler<OrderPaidDomainEvent>
{
    private readonly ISender _sender;
    private readonly IAppNotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;
    private readonly ITelegramNotifier _telegramNotifier;
    private readonly ILogger<NotifyOnOrderLifecycleEventsHandler> _logger;

    public NotifyOnOrderLifecycleEventsHandler(
        ISender sender,
        IAppNotificationRepository repository,
        INotificationUnitOfWork unitOfWork,
        ITelegramNotifier telegramNotifier,
        ILogger<NotifyOnOrderLifecycleEventsHandler> logger)
    {
        _sender = sender;
        _repository = repository;
        _unitOfWork = unitOfWork;
        _telegramNotifier = telegramNotifier;
        _logger = logger;
    }

    public Task Handle(OrderApprovedDomainEvent notification, CancellationToken cancellationToken) =>
        NotifyBuyerAsync(notification.OrderId, NotificationType.OrderApproved, "Order approved",
            id => $"Your order #{id} has been approved and is being prepared.", cancellationToken);

    public Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken) =>
        NotifyBuyerAsync(notification.OrderId, NotificationType.OrderCancelled, "Order cancelled",
            id => $"Your order #{id} has been cancelled.", cancellationToken);

    public Task Handle(OrderShippedDomainEvent notification, CancellationToken cancellationToken) =>
        NotifyBuyerAsync(notification.OrderId, NotificationType.OrderShipped, "Order shipped",
            id => $"Your order #{id} is on its way!", cancellationToken);

    public Task Handle(OrderCompletedDomainEvent notification, CancellationToken cancellationToken) =>
        NotifyBuyerAsync(notification.OrderId, NotificationType.OrderCompleted, "Order delivered",
            id => $"Your order #{id} has been delivered. Thank you for shopping with us!", cancellationToken);

    public Task Handle(OrderReturnedDomainEvent notification, CancellationToken cancellationToken) =>
        NotifyBuyerAsync(notification.OrderId, NotificationType.OrderReturned, "Return processed",
            id => $"Your return for order #{id} has been processed.", cancellationToken);

    public Task Handle(OrderPaidDomainEvent notification, CancellationToken cancellationToken) =>
        NotifyBuyerAsync(notification.OrderId, NotificationType.OrderPaid, "Payment confirmed",
            id => $"Payment for order #{id} has been confirmed.", cancellationToken);

    private async Task NotifyBuyerAsync(
        Guid orderId,
        NotificationType type,
        string title,
        Func<string, string> buildMessage,
        CancellationToken cancellationToken)
    {
        var order = await _sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        if (order is null)
            return;

        var shortId = order.Id.ToString()[..8].ToUpperInvariant();
        var message = buildMessage(shortId);

        _repository.Add(AppNotification.Create(order.UserId, type, title, message, relatedOrderId: order.Id));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var telegramId = await _sender.Send(new GetUserTelegramIdQuery(order.UserId), cancellationToken);
        if (telegramId is null)
            return;

        try
        {
            await _telegramNotifier.SendAsync(telegramId, $"🔔 {title}\n{message}", cancellationToken);
        }
        catch (Exception ex)
        {
            // Telegram delivery is best-effort — the in-app row above is already saved, so a push
            // failure (network blip, user blocked the bot, etc.) must never surface as an error to
            // the order-status-change request that triggered this handler.
            _logger.LogWarning(ex, "Failed to push Telegram notification for order {OrderId}", orderId);
        }
    }
}
