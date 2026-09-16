using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, Result<string>>
{
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public UploadProductImageCommandHandler(
        IProductRepository productRepository,
        IFileStorageService fileStorageService,
        ICatalogUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<string>(new Error("Product.NotFound", "Product not found."));

        var previousImageUrl = product.ImageUrl;

        var imageUrl = await _fileStorageService.SaveProductImageAsync(
            request.ProductId, request.Content, request.FileName, cancellationToken);

        product.UpdateImage(imageUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Every upload now lands on its own uniquely-named file (see LocalFileStorageService), so
        // replacing the cover photo would otherwise orphan the previous one on disk forever.
        if (previousImageUrl is not null)
            await _fileStorageService.DeleteAsync(previousImageUrl, cancellationToken);

        return Result.Success(imageUrl);
    }
}
