using MediatR;
using OnlineShop.Modules.Catalog.Domain.Events;
using OnlineShop.Modules.Inventory.Application.Abstractions;
using OnlineShop.Modules.Inventory.Domain;

namespace OnlineShop.Modules.Inventory.Application.EventHandlers;

/// <summary>
/// Reacts to Catalog.Product.Create() — every new product gets a zero-quantity stock record the
/// moment it exists, so "receiving stock" always has something to add to. Runs in the same in-process
/// MediatR publish as the Catalog command, but against Inventory's own DbContext/transaction
/// (Modular Monolith: separate schemas, separate unit of work per module — see AppDbContextBase).
/// </summary>
public sealed class InitializeProductStockOnProductCreatedHandler : INotificationHandler<ProductCreatedDomainEvent>
{
    private readonly IProductStockRepository _productStockRepository;
    private readonly IInventoryUnitOfWork _unitOfWork;

    public InitializeProductStockOnProductCreatedHandler(
        IProductStockRepository productStockRepository,
        IInventoryUnitOfWork unitOfWork)
    {
        _productStockRepository = productStockRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        if (await _productStockRepository.ExistsForProductAsync(notification.ProductId, cancellationToken))
            return;

        _productStockRepository.Add(ProductStock.Initialize(notification.ProductId));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
