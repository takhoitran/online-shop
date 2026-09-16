using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Orders;

/// <summary>ProductIds selects which cart lines to actually check out — null/empty means "the
/// whole cart" (keeps the Telegram bot, which has no item-selection UI, working unchanged).
/// Whatever isn't selected stays in the cart afterward instead of being cleared.</summary>
public sealed record CheckoutCommand(
    Guid UserId,
    string PaymentMethod,
    string RecipientName,
    string PhoneNumber,
    string AddressLine,
    string City,
    string? VoucherCode = null,
    IReadOnlyList<Guid>? ProductIds = null) : IRequest<Result<Guid>>;
