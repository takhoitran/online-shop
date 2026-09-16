using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Wishlists;

public sealed class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, Result>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;

    public RemoveFromWishlistCommandHandler(IWishlistRepository wishlistRepository, IOrderingUnitOfWork unitOfWork)
    {
        _wishlistRepository = wishlistRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var wishlist = await _wishlistRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wishlist is null)
            return Result.Success();

        wishlist.RemoveItem(request.ProductId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
