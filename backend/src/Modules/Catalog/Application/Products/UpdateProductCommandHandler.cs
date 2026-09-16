using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;
using OnlineShop.Modules.Catalog.Domain.ValueObjects;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ICatalogUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure(new Error("Product.NotFound", "Product not found."));

        if (!await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
            return Result.Failure(new Error("Category.NotFound", "Category does not exist."));

        product.UpdateDetails(
            request.Name,
            request.Description,
            Money.Of(request.Price),
            request.CategoryId,
            request.NameEn,
            request.NameVi,
            request.DescriptionEn,
            request.DescriptionVi);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
