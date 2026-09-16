using MediatR;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Ordering.Application.Dtos;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Carts;

public sealed class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly ICartRepository _cartRepository;
    private readonly ISender _sender;

    public GetCartQueryHandler(ICartRepository cartRepository, ISender sender)
    {
        _cartRepository = cartRepository;
        _sender = sender;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
            return new CartDto(request.UserId, [], 0, "VND");

        var lines = new List<CartItemLineDto>();
        foreach (var item in cart.Items)
        {
            var product = await _sender.Send(new GetProductByIdQuery(item.ProductId), cancellationToken);
            if (product is null)
                continue; // Product was removed from Catalog — skip it instead of breaking the whole cart.

            lines.Add(new CartItemLineDto(
                product.Id, product.Name, product.ImageUrl, product.Price, product.Currency,
                item.Quantity, product.Price * item.Quantity));
        }

        var currency = lines.Count > 0 ? lines[0].Currency : "VND";
        return new CartDto(request.UserId, lines, lines.Sum(l => l.LineTotal), currency);
    }
}
