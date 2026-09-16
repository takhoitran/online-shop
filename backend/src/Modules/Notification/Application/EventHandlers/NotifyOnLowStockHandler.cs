using MediatR;
using Microsoft.Extensions.Logging;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Identity.Application.Users;
using OnlineShop.Modules.Inventory.Domain.Events;
using OnlineShop.Modules.Notification.Application.Abstractions;
using OnlineShop.Modules.Notification.Domain;

namespace OnlineShop.Modules.Notification.Application.EventHandlers;

/// <summary>
/// Alerts every Seller/Admin (there's no single "owner" per product in this system, so the whole
/// staff gets it — see the plan's Phase 9 note) when a product's stock drops to/under its low-stock
/// threshold. Same durable-in-app + best-effort-Telegram pattern as order lifecycle notifications.
/// </summary>
public sealed class NotifyOnLowStockHandler : INotificationHandler<LowStockDetectedDomainEvent>
{
    private readonly ISender _sender;
    private readonly IAppNotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;
    private readonly ITelegramNotifier _telegramNotifier;
    private readonly ILogger<NotifyOnLowStockHandler> _logger;

    public NotifyOnLowStockHandler(
        ISender sender,
        IAppNotificationRepository repository,
        INotificationUnitOfWork unitOfWork,
        ITelegramNotifier telegramNotifier,
        ILogger<NotifyOnLowStockHandler> logger)
    {
        _sender = sender;
        _repository = repository;
        _unitOfWork = unitOfWork;
        _telegramNotifier = telegramNotifier;
        _logger = logger;
    }

    public async Task Handle(LowStockDetectedDomainEvent notification, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductByIdQuery(notification.ProductId), cancellationToken);
        if (product is null)
            return;

        const string title = "Low stock alert";
        var message = $"{product.Name} is running low — only {notification.QuantityOnHand} left in stock.";

        var users = await _sender.Send(new GetUsersQuery(), cancellationToken);
        var staff = users.Where(u => u.Role is "Seller" or "Admin");

        foreach (var member in staff)
        {
            _repository.Add(AppNotification.Create(member.Id, NotificationType.LowStock, title, message, relatedProductId: product.Id));
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var member in staff)
        {
            var telegramId = await _sender.Send(new GetUserTelegramIdQuery(member.Id), cancellationToken);
            if (telegramId is null)
                continue;

            try
            {
                await _telegramNotifier.SendAsync(telegramId, $"🔔 {title}\n{message}", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to push low-stock Telegram notification to user {UserId}", member.Id);
            }
        }
    }
}
