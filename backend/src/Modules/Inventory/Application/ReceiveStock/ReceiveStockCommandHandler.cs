using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Inventory.Application.Abstractions;
using OnlineShop.Modules.Inventory.Domain;

namespace OnlineShop.Modules.Inventory.Application.ReceiveStock;

public sealed class ReceiveStockCommandHandler : IRequestHandler<ReceiveStockCommand, Result>
{
    private readonly IProductStockRepository _productStockRepository;
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public ReceiveStockCommandHandler(
        IProductStockRepository productStockRepository,
        IInventoryUnitOfWork unitOfWork,
        ISender sender)
    {
        _productStockRepository = productStockRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task<Result> Handle(ReceiveStockCommand request, CancellationToken cancellationToken)
    {
        var stock = await _productStockRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

        if (stock is null)
        {
            // Self-heals products that existed before Inventory did (or if the ProductCreated
            // event handler hasn't run yet) — but only for a product that really exists in
            // Catalog, via the Open Host Service query, never for an arbitrary/typo'd id.
            var product = await _sender.Send(new GetProductByIdQuery(request.ProductId), cancellationToken);
            if (product is null)
                return Result.Failure(new Error("Product.NotFound", "Product not found to receive stock for."));

            stock = ProductStock.Initialize(request.ProductId);
            _productStockRepository.Add(stock);
        }

        stock.ReceiveStock(request.Quantity, request.PerformedByUserId, request.Note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
