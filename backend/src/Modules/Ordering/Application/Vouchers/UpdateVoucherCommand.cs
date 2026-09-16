using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed record UpdateVoucherCommand(
    Guid VoucherId,
    string DiscountType,
    decimal DiscountValue,
    int? MaxUses,
    DateTime? ExpiresAtUtc) : IRequest<Result>;
