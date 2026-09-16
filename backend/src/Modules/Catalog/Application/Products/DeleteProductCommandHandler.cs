using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public DeleteProductCommandHandler(
        IProductRepository productRepository, ICatalogUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure(new Error("Product.NotFound", "Product not found."));

        // Capture every image URL before the rows are gone — nothing else references them once
        // the product itself is deleted.
        var imageUrls = new List<string>();
        if (product.ImageUrl is not null)
            imageUrls.Add(product.ImageUrl);
        imageUrls.AddRange(product.Images.Select(i => i.Url));

        // Existing orders keep their own frozen name/price snapshot (OrderDetail has no FK back to
        // Product), and a cart referencing this product already skips it gracefully if missing
        // (see GetCartQueryHandler) — deleting is safe with no further checks needed here.
        product.Delete();
        _productRepository.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var url in imageUrls)
            await _fileStorageService.DeleteAsync(url, cancellationToken);

        return Result.Success();
    }
}
