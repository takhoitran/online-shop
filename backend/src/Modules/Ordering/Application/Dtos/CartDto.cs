namespace OnlineShop.Modules.Ordering.Application.Dtos;

public sealed record CartItemLineDto(
    Guid ProductId,
    string ProductName,
    string? ImageUrl,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);

public sealed record CartDto(Guid UserId, IReadOnlyList<CartItemLineDto> Items, decimal Total, string Currency);
