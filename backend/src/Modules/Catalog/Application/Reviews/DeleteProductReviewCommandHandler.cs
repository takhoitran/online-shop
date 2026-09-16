using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Reviews;

public sealed class DeleteProductReviewCommandHandler : IRequestHandler<DeleteProductReviewCommand, Result>
{
    private readonly IProductReviewRepository _reviewRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public DeleteProductReviewCommandHandler(IProductReviewRepository reviewRepository, ICatalogUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteProductReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null)
            return Result.Failure(new Error("Review.NotFound", "Review not found."));

        _reviewRepository.Remove(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
