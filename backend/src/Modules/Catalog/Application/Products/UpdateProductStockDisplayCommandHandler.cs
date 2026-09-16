using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed class UpdateProductStockDisplayCommandHandler : IRequestHandler<UpdateProductStockDisplayCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public UpdateProductStockDisplayCommandHandler(IProductRepository productRepository, ICatalogUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProductStockDisplayCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure(new Error("Product.NotFound", "Product not found to sync stock display for."));

        product.SyncStockDisplay(request.QuantityOnHand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
