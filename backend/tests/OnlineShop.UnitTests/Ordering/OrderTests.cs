using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Ordering.Domain;
using OnlineShop.Modules.Ordering.Domain.Events;
using OnlineShop.Modules.Ordering.Domain.ValueObjects;

namespace OnlineShop.UnitTests.Ordering;

public class OrderTests
{
    private static OrderLineInput Line(int quantity = 1, decimal unitPrice = 100_000) =>
        new(Guid.NewGuid(), "Test Product", quantity, Money.Of(unitPrice));

    private static ShippingAddress TestAddress() =>
        ShippingAddress.Of("Test Buyer", "0900000000", "123 Test Street", "Test City");

    private static Order PlaceOrder(params OrderLineInput[] lines) =>
        Order.Place(Guid.NewGuid(), lines, PaymentMethod.Cod, TestAddress());

    [Fact]
    public void Place_WithNoLines_Throws()
    {
        Assert.Throws<DomainException>(() => Order.Place(Guid.NewGuid(), [], PaymentMethod.Cod, TestAddress()));
    }

    [Fact]
    public void Place_ComputesTotalAsSumOfLines()
    {
        var order = PlaceOrder(Line(quantity: 2, unitPrice: 100_000), Line(quantity: 1, unitPrice: 50_000));

        Assert.Equal(250_000, order.TotalAmount.Amount);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Place_WithNoDiscount_SubtotalEqualsTotal()
    {
        var order = Order.Place(Guid.NewGuid(), [Line(quantity: 1, unitPrice: 100_000)], PaymentMethod.Cod, TestAddress());

        Assert.Equal(100_000, order.Subtotal.Amount);
        Assert.Equal(0, order.DiscountAmount.Amount);
        Assert.Equal(100_000, order.TotalAmount.Amount);
        Assert.Null(order.VoucherCode);
    }

    [Fact]
    public void Place_WithDiscount_TotalIsSubtotalMinusDiscount()
    {
        var order = Order.Place(
            Guid.NewGuid(), [Line(quantity: 1, unitPrice: 100_000)], PaymentMethod.Cod, TestAddress(),
            discountAmount: Money.Of(30_000), voucherCode: "SAVE30K");

        Assert.Equal(100_000, order.Subtotal.Amount);
        Assert.Equal(30_000, order.DiscountAmount.Amount);
        Assert.Equal(70_000, order.TotalAmount.Amount);
        Assert.Equal("SAVE30K", order.VoucherCode);
    }

    [Fact]
    public void Place_WithDiscountLargerThanSubtotal_TotalIsClampedAtZero()
    {
        var order = Order.Place(
            Guid.NewGuid(), [Line(quantity: 1, unitPrice: 50_000)], PaymentMethod.Cod, TestAddress(),
            discountAmount: Money.Of(999_999), voucherCode: "HUGE");

        Assert.Equal(0, order.TotalAmount.Amount);
    }

    [Fact]
    public void Place_RaisesOrderPlacedDomainEvent()
    {
        var order = PlaceOrder(Line());

        var raised = Assert.Single(order.DomainEvents);
        var placed = Assert.IsType<OrderPlacedDomainEvent>(raised);
        Assert.Equal(order.Id, placed.OrderId);
        Assert.Equal(order.UserId, placed.BuyerUserId);
    }

    [Fact]
    public void Approve_WithSufficientStock_TransitionsToApprovedAndRaisesEvent()
    {
        var line = Line(quantity: 3);
        var order = PlaceOrder(line);
        order.ClearDomainEvents();

        order.Approve(Guid.NewGuid(), new Dictionary<Guid, int> { [line.ProductId] = 3 });

        Assert.Equal(OrderStatus.Approved, order.Status);
        Assert.IsType<OrderApprovedDomainEvent>(Assert.Single(order.DomainEvents));
    }

    [Fact]
    public void Approve_WithInsufficientStock_Throws()
    {
        var line = Line(quantity: 5);
        var order = PlaceOrder(line);

        var ex = Assert.Throws<DomainException>(() =>
            order.Approve(Guid.NewGuid(), new Dictionary<Guid, int> { [line.ProductId] = 4 }));

        Assert.Contains("enough stock", ex.Message);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Approve_WhenNotPending_Throws()
    {
        var line = Line(quantity: 1);
        var order = PlaceOrder(line);
        order.Approve(Guid.NewGuid(), new Dictionary<Guid, int> { [line.ProductId] = 1 });

        Assert.Throws<DomainException>(() =>
            order.Approve(Guid.NewGuid(), new Dictionary<Guid, int> { [line.ProductId] = 1 }));
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Approved)]
    public void Cancel_FromPendingOrApproved_Succeeds(OrderStatus fromStatus)
    {
        var line = Line(quantity: 1);
        var order = PlaceOrder(line);
        if (fromStatus == OrderStatus.Approved)
            order.Approve(Guid.NewGuid(), new Dictionary<Guid, int> { [line.ProductId] = 1 });

        order.Cancel(Guid.NewGuid(), "changed my mind");

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_CarriesWasApprovedFlag_ForInventoryToDecideWhetherToRestock()
    {
        var line = Line(quantity: 1);
        var order = PlaceOrder(line);
        order.Approve(Guid.NewGuid(), new Dictionary<Guid, int> { [line.ProductId] = 1 });
        order.ClearDomainEvents();

        order.Cancel(Guid.NewGuid(), null);

        var cancelled = Assert.IsType<OrderCancelledDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.True(cancelled.WasApproved);
    }

    [Fact]
    public void Cancel_WhenShippingOrLater_Throws()
    {
        var line = Line(quantity: 1);
        var order = PlaceOrder(line);
        order.Approve(Guid.NewGuid(), new Dictionary<Guid, int> { [line.ProductId] = 1 });
        order.Ship();

        Assert.Throws<DomainException>(() => order.Cancel(Guid.NewGuid(), null));
    }

    [Fact]
    public void FullLifecycle_PendingToApprovedToShippingToCompletedToReturned_Succeeds()
    {
        var line = Line(quantity: 1);
        var order = PlaceOrder(line);

        order.Approve(Guid.NewGuid(), new Dictionary<Guid, int> { [line.ProductId] = 1 });
        Assert.Equal(OrderStatus.Approved, order.Status);

        order.Ship();
        Assert.Equal(OrderStatus.Shipping, order.Status);

        order.Complete();
        Assert.Equal(OrderStatus.Completed, order.Status);

        order.Return(Guid.NewGuid(), "defective");
        Assert.Equal(OrderStatus.Returned, order.Status);
    }

    [Fact]
    public void Ship_WhenNotApproved_Throws()
    {
        var order = PlaceOrder(Line());
        Assert.Throws<DomainException>(() => order.Ship());
    }

    [Fact]
    public void Complete_WhenNotShipping_Throws()
    {
        var line = Line(quantity: 1);
        var order = PlaceOrder(line);
        order.Approve(Guid.NewGuid(), new Dictionary<Guid, int> { [line.ProductId] = 1 });

        Assert.Throws<DomainException>(() => order.Complete());
    }

    [Fact]
    public void Return_WhenNotCompleted_Throws()
    {
        var order = PlaceOrder(Line());
        Assert.Throws<DomainException>(() => order.Return(Guid.NewGuid(), null));
    }

    [Fact]
    public void MarkAsPaid_Twice_ThrowsOnSecondCall()
    {
        var order = PlaceOrder(Line());
        order.MarkAsPaid();

        Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);
        Assert.Throws<DomainException>(() => order.MarkAsPaid());
    }

    [Fact]
    public void MarkAsPaid_AfterCancellation_Throws()
    {
        var order = PlaceOrder(Line());
        order.Cancel(Guid.NewGuid(), null);

        Assert.Throws<DomainException>(() => order.MarkAsPaid());
    }
}
