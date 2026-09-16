using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed class AddProductGalleryImageCommandHandler : IRequestHandler<AddProductGalleryImageCommand, Result<string>>
{
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public AddProductGalleryImageCommandHandler(
        IProductRepository productRepository,
        IFileStorageService fileStorageService,
        ICatalogUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(AddProductGalleryImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<string>(new Error("Product.NotFound", "Product not found."));

        var imageUrl = await _fileStorageService.SaveProductImageAsync(
            request.ProductId, request.Content, request.FileName, cancellationToken);

        product.AddImage(imageUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(imageUrl);
    }
}
