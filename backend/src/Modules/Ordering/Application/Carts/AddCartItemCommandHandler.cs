using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Carts;

public sealed class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public AddCartItemCommandHandler(ICartRepository cartRepository, IOrderingUnitOfWork unitOfWork, ISender sender)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task<Result> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductByIdQuery(request.ProductId), cancellationToken);
        if (product is null)
            return Result.Failure(new Error("Product.NotFound", "Product not found."));

        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null)
        {
            cart = Cart.CreateFor(request.UserId);
            _cartRepository.Add(cart);
        }

        // A soft, buyer-facing check only — the real deduction/enforcement still happens at
        // Approve time against the same stock cache (see Order.Approve). This just stops a buyer
        // from filling their cart with a quantity they'll only discover is impossible once staff
        // rejects the order days later.
        var alreadyInCart = cart.Items.SingleOrDefault(i => i.ProductId == request.ProductId)?.Quantity ?? 0;
        if (alreadyInCart + request.Quantity > product.StockQuantity)
        {
            var suffix = alreadyInCart > 0 ? $" — you already have {alreadyInCart} in your cart." : ".";
            return Result.Failure(new Error(
                "Product.InsufficientStock",
                $"Only {product.StockQuantity} of \"{product.Name}\" in stock{suffix}"));
        }

        cart.AddItem(request.ProductId, request.Quantity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
