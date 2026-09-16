using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Orders;

/// <summary>
/// Stock sufficiency is checked here against Catalog's synced StockQuantity display cache
/// (Customer/Supplier — Ordering never queries Inventory directly, matching the Context Map).
/// The authoritative deduction still happens in Inventory when it reacts to OrderApprovedDomainEvent,
/// which re-validates against its own real ProductStock — a rare stale-cache race would surface
/// there as a failed stock issuance after this Approve already committed. Accepting that edge case
/// (rather than a distributed-transaction/saga) is a deliberate scope trade-off for this project.
/// </summary>
public sealed class ApproveOrderCommandHandler : IRequestHandler<ApproveOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public ApproveOrderCommandHandler(IOrderRepository orderRepository, IOrderingUnitOfWork unitOfWork, ISender sender)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task<Result> Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(new Error("Order.NotFound", "Order not found."));

        var availableStock = new Dictionary<Guid, int>();
        foreach (var detail in order.Details)
        {
            if (availableStock.ContainsKey(detail.ProductId))
                continue;

            var product = await _sender.Send(new GetProductByIdQuery(detail.ProductId), cancellationToken);
            availableStock[detail.ProductId] = product?.StockQuantity ?? 0;
        }

        try
        {
            order.Approve(request.ApprovedByUserId, availableStock);
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error("Order.InvalidOperation", ex.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
