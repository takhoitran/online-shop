using MediatR;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Ordering.Application.Dtos;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Wishlists;

public sealed class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, WishlistDto>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ISender _sender;

    public GetWishlistQueryHandler(IWishlistRepository wishlistRepository, ISender sender)
    {
        _wishlistRepository = wishlistRepository;
        _sender = sender;
    }

    public async Task<WishlistDto> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var wishlist = await _wishlistRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wishlist is null || wishlist.Items.Count == 0)
            return new WishlistDto(request.UserId, []);

        var lines = new List<WishlistItemDto>();
        foreach (var item in wishlist.Items.OrderByDescending(i => i.AddedAtUtc))
        {
            var product = await _sender.Send(new GetProductByIdQuery(item.ProductId), cancellationToken);
            if (product is null)
                continue; // Product was removed from Catalog — skip it instead of breaking the whole wishlist.

            lines.Add(new WishlistItemDto(
                product.Id, product.Name, product.ImageUrl, product.Price, product.Currency,
                product.StockQuantity, item.AddedAtUtc));
        }

        return new WishlistDto(request.UserId, lines);
    }
}
