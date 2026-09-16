using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Ordering.Domain.Events;
using OnlineShop.Modules.Ordering.Domain.ValueObjects;

namespace OnlineShop.Modules.Ordering.Domain;

/// <summary>
/// Aggregate root for the checkout-through-fulfillment lifecycle. Order and OrderDetail are one
/// Aggregate (not split like Product/ProductStock) because TotalAmount must always equal the sum
/// of its lines — that invariant has to be enforced inside a single transactional boundary.
/// </summary>
public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderDetail> _details = new();

    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public Money Subtotal { get; private set; } = default!;
    public Money DiscountAmount { get; private set; } = default!;
    public string? VoucherCode { get; private set; }
    public Money TotalAmount { get; private set; } = default!;
    public ShippingAddress ShippingAddress { get; private set; } = default!;
    public DateTime OrderDateUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<OrderDetail> Details => _details.AsReadOnly();

    private Order() { }

    private Order(Guid id, Guid userId, PaymentMethod paymentMethod, ShippingAddress shippingAddress) : base(id)
    {
        UserId = userId;
        PaymentMethod = paymentMethod;
        PaymentStatus = PaymentStatus.Unpaid;
        Status = OrderStatus.Pending;
        ShippingAddress = shippingAddress;
        OrderDateUtc = DateTime.UtcNow;
        UpdatedAtUtc = OrderDateUtc;
    }

    public static Order Place(
        Guid userId,
        IReadOnlyList<OrderLineInput> lines,
        PaymentMethod paymentMethod,
        ShippingAddress shippingAddress,
        Money? discountAmount = null,
        string? voucherCode = null)
    {
        if (lines.Count == 0)
            throw new DomainException("An order must have at least one item.");

        var order = new Order(Guid.NewGuid(), userId, paymentMethod, shippingAddress);

        foreach (var line in lines)
            order._details.Add(OrderDetail.Create(line.ProductId, line.ProductName, line.Quantity, line.UnitPrice));

        order.RecalculateTotal(discountAmount, voucherCode);
        order.Raise(new OrderPlacedDomainEvent(order.Id, userId));
        return order;
    }

    /// <summary>availableStockByProductId comes from Catalog's synced StockQuantity (fetched by
    /// the Application handler) — the Domain enforces the rule, the Application fetches the data.</summary>
    public void Approve(Guid approvedByUserId, IReadOnlyDictionary<Guid, int> availableStockByProductId)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only a Pending order can be approved.");

        foreach (var detail in _details)
        {
            var available = availableStockByProductId.GetValueOrDefault(detail.ProductId, 0);
            if (detail.Quantity > available)
                throw new DomainException($"Product \"{detail.ProductName}\" doesn't have enough stock to approve this order.");
        }

        Status = OrderStatus.Approved;
        Touch();
        Raise(new OrderApprovedDomainEvent(Id, approvedByUserId, ToLineItems()));
    }

    public void Cancel(Guid cancelledByUserId, string? reason)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Approved))
            throw new DomainException("This order cannot be cancelled in its current status.");

        var wasApproved = Status == OrderStatus.Approved;
        Status = OrderStatus.Cancelled;
        Touch();
        Raise(new OrderCancelledDomainEvent(Id, cancelledByUserId, wasApproved, ToLineItems()));
    }

    public void MarkAsPaid()
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Returned)
            throw new DomainException("Cannot confirm payment for a cancelled/returned order.");
        if (PaymentStatus == PaymentStatus.Paid)
            throw new DomainException("This order's payment was already confirmed.");

        PaymentStatus = PaymentStatus.Paid;
        Touch();
        Raise(new OrderPaidDomainEvent(Id));
    }

    public void Ship()
    {
        if (Status != OrderStatus.Approved)
            throw new DomainException("Only an Approved order can move to Shipping.");

        Status = OrderStatus.Shipping;
        Touch();
        Raise(new OrderShippedDomainEvent(Id));
    }

    public void Complete()
    {
        if (Status != OrderStatus.Shipping)
            throw new DomainException("Only a Shipping order can be completed.");

        Status = OrderStatus.Completed;
        Touch();
        Raise(new OrderCompletedDomainEvent(Id));
    }

    public void Return(Guid processedByUserId, string? reason)
    {
        if (Status != OrderStatus.Completed)
            throw new DomainException("Only a Completed order can be processed as a return.");

        Status = OrderStatus.Returned;
        Touch();
        Raise(new OrderReturnedDomainEvent(Id, processedByUserId, ToLineItems()));
    }

    private void RecalculateTotal(Money? discountAmount, string? voucherCode)
    {
        Subtotal = _details.Aggregate(Money.Zero(), (total, detail) => total.Add(detail.LineTotal));
        DiscountAmount = discountAmount ?? Money.Zero(Subtotal.Currency);
        VoucherCode = voucherCode;
        TotalAmount = Subtotal.Subtract(DiscountAmount);
    }

    private List<OrderLineItem> ToLineItems() =>
        _details.Select(d => new OrderLineItem(d.ProductId, d.Quantity)).ToList();

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
