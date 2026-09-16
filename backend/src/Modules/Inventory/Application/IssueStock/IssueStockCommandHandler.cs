using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Inventory.Application.Abstractions;
using OnlineShop.Modules.Inventory.Domain;

namespace OnlineShop.Modules.Inventory.Application.IssueStock;

public sealed class IssueStockCommandHandler : IRequestHandler<IssueStockCommand, Result>
{
    private readonly IProductStockRepository _productStockRepository;
    private readonly IInventoryUnitOfWork _unitOfWork;

    public IssueStockCommandHandler(IProductStockRepository productStockRepository, IInventoryUnitOfWork unitOfWork)
    {
        _productStockRepository = productStockRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(IssueStockCommand request, CancellationToken cancellationToken)
    {
        var stock = await _productStockRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
        if (stock is null)
            return Result.Failure(new Error("Inventory.NotFound", "This product has no stock record yet."));

        try
        {
            stock.IssueStock(request.Quantity, request.PerformedByUserId, orderId: null, request.Note);
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error("Inventory.InvalidOperation", ex.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
