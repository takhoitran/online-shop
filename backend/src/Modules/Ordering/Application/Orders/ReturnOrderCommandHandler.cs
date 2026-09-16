using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed class ReturnOrderCommandHandler : IRequestHandler<ReturnOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;

    public ReturnOrderCommandHandler(IOrderRepository orderRepository, IVoucherRepository voucherRepository, IOrderingUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _voucherRepository = voucherRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReturnOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(new Error("Order.NotFound", "Order not found."));

        try
        {
            order.Return(request.ProcessedByUserId, request.Reason);
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error("Order.InvalidOperation", ex.Message));
        }

        // Same reasoning as CancelOrderCommandHandler — a returned order's discount is no longer
        // in effect, so free up the voucher use.
        if (order.VoucherCode is not null)
        {
            var voucher = await _voucherRepository.GetByCodeAsync(order.VoucherCode, cancellationToken);
            voucher?.ReleaseUse();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
