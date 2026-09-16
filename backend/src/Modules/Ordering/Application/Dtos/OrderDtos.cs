namespace OnlineShop.Modules.Ordering.Application.Dtos;

public sealed record OrderDetailLineDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record OrderDto(
    Guid Id,
    Guid UserId,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    decimal Subtotal,
    decimal DiscountAmount,
    string? VoucherCode,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<OrderDetailLineDto> Details,
    DateTime OrderDateUtc,
    DateTime UpdatedAtUtc,
    string RecipientName,
    string PhoneNumber,
    string AddressLine,
    string City);

public sealed record OrderSummaryDto(
    Guid Id,
    Guid UserId,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    decimal TotalAmount,
    string Currency,
    int ItemCount,
    DateTime OrderDateUtc);
