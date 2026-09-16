using MediatR;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed record PreviewVoucherQuery(Guid UserId, string Code, IReadOnlyList<Guid>? ProductIds = null) : IRequest<VoucherPreviewDto>;
