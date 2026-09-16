using MediatR;
using OnlineShop.Modules.Catalog.Domain.Events;
using OnlineShop.Modules.Inventory.Application.Abstractions;
using OnlineShop.Modules.Inventory.Domain;

namespace OnlineShop.Modules.Inventory.Application.EventHandlers;

/// <summary>Mirrors InitializeProductStockOnProductCreatedHandler in the other direction — a
/// deleted product's stock row (and its transaction history, cascading) would otherwise sit
/// orphaned forever with no product to belong to.</summary>
public sealed class RemoveStockOnProductDeletedHandler : INotificationHandler<ProductDeletedDomainEvent>
{
    private readonly IProductStockRepository _productStockRepository;
    private readonly IInventoryUnitOfWork _unitOfWork;

    public RemoveStockOnProductDeletedHandler(IProductStockRepository productStockRepository, IInventoryUnitOfWork unitOfWork)
    {
        _productStockRepository = productStockRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProductDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var stock = await _productStockRepository.GetByProductIdAsync(notification.ProductId, cancellationToken);
        if (stock is null)
            return;

        _productStockRepository.Remove(stock);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
