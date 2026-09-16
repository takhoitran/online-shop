using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Wishlists;

public sealed class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, Result>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public AddToWishlistCommandHandler(IWishlistRepository wishlistRepository, IOrderingUnitOfWork unitOfWork, ISender sender)
    {
        _wishlistRepository = wishlistRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task<Result> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductByIdQuery(request.ProductId), cancellationToken);
        if (product is null)
            return Result.Failure(new Error("Product.NotFound", "Product not found."));

        var wishlist = await _wishlistRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wishlist is null)
        {
            wishlist = Wishlist.CreateFor(request.UserId);
            _wishlistRepository.Add(wishlist);
        }

        wishlist.AddItem(request.ProductId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
