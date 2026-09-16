using MediatR;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed record GetPublicVouchersQuery : IRequest<List<PublicVoucherDto>>;
