using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed class DeactivateVoucherCommandHandler : IRequestHandler<DeactivateVoucherCommand, Result>
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;

    public DeactivateVoucherCommandHandler(IVoucherRepository voucherRepository, IOrderingUnitOfWork unitOfWork)
    {
        _voucherRepository = voucherRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId, cancellationToken);
        if (voucher is null)
            return Result.Failure(new Error("Voucher.NotFound", "Voucher not found."));

        voucher.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
