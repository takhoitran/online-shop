using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(IOrderRepository orderRepository, IVoucherRepository voucherRepository, IOrderingUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _voucherRepository = voucherRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(new Error("Order.NotFound", "Order not found."));

        if (!request.IsStaff && order.UserId != request.RequestedByUserId)
            return Result.Failure(new Error("Order.Forbidden", "You don't have permission to cancel this order."));

        try
        {
            order.Cancel(request.RequestedByUserId, request.Reason);
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error("Order.InvalidOperation", ex.Message));
        }

        // The discount was never actually redeemed if the order never went through — free up the
        // use so it doesn't permanently count against a limited voucher's MaxUses.
        if (order.VoucherCode is not null)
        {
            var voucher = await _voucherRepository.GetByCodeAsync(order.VoucherCode, cancellationToken);
            voucher?.ReleaseUse();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
