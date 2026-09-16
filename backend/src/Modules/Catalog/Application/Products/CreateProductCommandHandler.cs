using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;
using OnlineShop.Modules.Catalog.Domain.ValueObjects;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ICatalogUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
            return Result.Failure<Guid>(new Error("Category.NotFound", "Category does not exist."));

        var product = Product.Create(
            request.Name,
            request.Description,
            Money.Of(request.Price),
            request.CategoryId,
            request.NameEn,
            request.NameVi,
            request.DescriptionEn,
            request.DescriptionVi);

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
