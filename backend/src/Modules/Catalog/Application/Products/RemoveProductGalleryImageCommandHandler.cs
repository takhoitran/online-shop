using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed class RemoveProductGalleryImageCommandHandler : IRequestHandler<RemoveProductGalleryImageCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public RemoveProductGalleryImageCommandHandler(
        IProductRepository productRepository, ICatalogUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(RemoveProductGalleryImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure(new Error("Product.NotFound", "Product not found."));

        var imageUrl = product.Images.SingleOrDefault(i => i.Id == request.ImageId)?.Url;

        try
        {
            product.RemoveImage(request.ImageId);
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error("Product.ImageNotFound", ex.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (imageUrl is not null)
            await _fileStorageService.DeleteAsync(imageUrl, cancellationToken);

        return Result.Success();
    }
}
