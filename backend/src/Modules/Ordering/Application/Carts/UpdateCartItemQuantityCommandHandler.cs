using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Carts;

public sealed class UpdateCartItemQuantityCommandHandler : IRequestHandler<UpdateCartItemQuantityCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public UpdateCartItemQuantityCommandHandler(ICartRepository cartRepository, IOrderingUnitOfWork unitOfWork, ISender sender)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task<Result> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null)
            return Result.Failure(new Error("Cart.NotFound", "Your cart is empty."));

        // Same soft buyer-facing check as AddCartItemCommandHandler — only applies when actually
        // raising the quantity; lowering it (or to 0, which removes the line) can never be blocked
        // by a stock ceiling.
        if (request.Quantity > 0)
        {
            var product = await _sender.Send(new GetProductByIdQuery(request.ProductId), cancellationToken);
            if (product is not null && request.Quantity > product.StockQuantity)
            {
                return Result.Failure(new Error(
                    "Product.InsufficientStock",
                    $"Only {product.StockQuantity} of \"{product.Name}\" in stock."));
            }
        }

        try
        {
            cart.UpdateItemQuantity(request.ProductId, request.Quantity);
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error("Cart.InvalidOperation", ex.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
