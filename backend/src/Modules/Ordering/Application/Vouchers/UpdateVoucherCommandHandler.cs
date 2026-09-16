using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, Result>
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;

    public UpdateVoucherCommandHandler(IVoucherRepository voucherRepository, IOrderingUnitOfWork unitOfWork)
    {
        _voucherRepository = voucherRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId, cancellationToken);
        if (voucher is null)
            return Result.Failure(new Error("Voucher.NotFound", "Voucher not found."));

        try
        {
            voucher.UpdateDetails(
                Enum.Parse<DiscountType>(request.DiscountType, ignoreCase: true),
                request.DiscountValue,
                request.MaxUses,
                request.ExpiresAtUtc);
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error("Voucher.Invalid", ex.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
