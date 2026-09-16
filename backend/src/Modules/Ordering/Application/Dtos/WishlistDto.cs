namespace OnlineShop.Modules.Ordering.Application.Dtos;

public sealed record WishlistItemDto(
    Guid ProductId,
    string ProductName,
    string? ImageUrl,
    decimal Price,
    string Currency,
    int StockQuantity,
    DateTime AddedAtUtc);

public sealed record WishlistDto(Guid UserId, IReadOnlyList<WishlistItemDto> Items);
