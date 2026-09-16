using MediatR;
using OnlineShop.Modules.Inventory.Application.Abstractions;
using OnlineShop.Modules.Inventory.Domain;
using OnlineShop.Modules.Ordering.Domain.Events;

namespace OnlineShop.Modules.Inventory.Application.EventHandlers;

/// <summary>
/// The real "Ordering --domain event--> Inventory" edge from the Context Map, now code: Ordering
/// never calls into Inventory directly, it only raises events describing what happened, and
/// Inventory decides what that means for stock. References Ordering.Domain only (event types),
/// never Ordering.Application — that's what keeps this from being a circular project reference,
/// since Ordering.Application already depends on this project (Inventory.Application) for the
/// stock-check query used by ApproveOrderCommandHandler.
/// </summary>
public sealed class AdjustStockOnOrderEventsHandler :
    INotificationHandler<OrderApprovedDomainEvent>,
    INotificationHandler<OrderCancelledDomainEvent>,
    INotificationHandler<OrderReturnedDomainEvent>
{
    private readonly IProductStockRepository _productStockRepository;
    private readonly IInventoryUnitOfWork _unitOfWork;

    public AdjustStockOnOrderEventsHandler(IProductStockRepository productStockRepository, IInventoryUnitOfWork unitOfWork)
    {
        _productStockRepository = productStockRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OrderApprovedDomainEvent notification, CancellationToken cancellationToken)
    {
        foreach (var line in notification.Lines)
        {
            var stock = await _productStockRepository.GetByProductIdAsync(line.ProductId, cancellationToken);
            stock?.IssueStock(line.Quantity, notification.ApprovedByUserId, notification.OrderId, note: "Issued for order");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.WasApproved)
            return; // Pending -> Cancelled never issued stock in the first place.

        foreach (var line in notification.Lines)
        {
            var stock = await _productStockRepository.GetByProductIdAsync(line.ProductId, cancellationToken);
            stock?.ReturnStock(line.Quantity, notification.CancelledByUserId, notification.OrderId, "Returned due to order cancelled after approval");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(OrderReturnedDomainEvent notification, CancellationToken cancellationToken)
    {
        foreach (var line in notification.Lines)
        {
            var stock = await _productStockRepository.GetByProductIdAsync(line.ProductId, cancellationToken);
            stock?.ReturnStock(line.Quantity, notification.ProcessedByUserId, notification.OrderId, "Returned by customer");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
