using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Reviews;

public sealed class CreateProductReviewCommandHandler : IRequestHandler<CreateProductReviewCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductReviewRepository _reviewRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public CreateProductReviewCommandHandler(
        IProductRepository productRepository,
        IProductReviewRepository reviewRepository,
        ICatalogUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProductReviewCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<Guid>(new Error("Product.NotFound", "Product not found."));

        var existing = await _reviewRepository.GetByProductAndBuyerAsync(request.ProductId, request.BuyerId, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(new Error("Review.AlreadyExists", "You have already reviewed this product."));

        var review = ProductReview.Create(request.ProductId, request.BuyerId, request.BuyerName, request.Rating, request.Comment);
        _reviewRepository.Add(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(review.Id);
    }
}
