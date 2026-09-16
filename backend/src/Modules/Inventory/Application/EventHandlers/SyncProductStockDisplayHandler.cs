using MediatR;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Inventory.Domain.Events;

namespace OnlineShop.Modules.Inventory.Application.EventHandlers;

/// <summary>
/// Keeps Catalog.Product.StockQuantity (a display cache — see its XML doc) in sync after every
/// stock movement. This is the "Inventory --published read model: StockLevel--> Catalog" edge
/// from the Context Map: Inventory pushes, Catalog never reaches into Inventory's own tables.
/// </summary>
public sealed class SyncProductStockDisplayHandler :
    INotificationHandler<StockReceivedDomainEvent>,
    INotificationHandler<StockIssuedDomainEvent>,
    INotificationHandler<StockReturnedDomainEvent>
{
    private readonly ISender _sender;

    public SyncProductStockDisplayHandler(ISender sender)
    {
        _sender = sender;
    }

    public Task Handle(StockReceivedDomainEvent notification, CancellationToken cancellationToken) =>
        Sync(notification.ProductId, notification.QuantityOnHand, cancellationToken);

    public Task Handle(StockIssuedDomainEvent notification, CancellationToken cancellationToken) =>
        Sync(notification.ProductId, notification.QuantityOnHand, cancellationToken);

    public Task Handle(StockReturnedDomainEvent notification, CancellationToken cancellationToken) =>
        Sync(notification.ProductId, notification.QuantityOnHand, cancellationToken);

    private Task Sync(Guid productId, int quantityOnHand, CancellationToken cancellationToken) =>
        _sender.Send(new UpdateProductStockDisplayCommand(productId, quantityOnHand), cancellationToken);
}
