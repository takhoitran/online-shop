using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;
using OnlineShop.Modules.Ordering.Domain.ValueObjects;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, Result<Guid>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public CheckoutCommandHandler(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IVoucherRepository voucherRepository,
        IOrderingUnitOfWork unitOfWork,
        ISender sender)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _voucherRepository = voucherRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task<Result<Guid>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
            return Result.Failure<Guid>(new Error("Cart.Empty", "Your cart is empty."));

        // No selection (e.g. the Telegram bot, which has no item-picker UI) means "check out
        // everything" — the pre-existing behavior.
        var itemsToCheckout = request.ProductIds is { Count: > 0 } selected
            ? cart.Items.Where(i => selected.Contains(i.ProductId)).ToList()
            : cart.Items.ToList();

        if (itemsToCheckout.Count == 0)
            return Result.Failure<Guid>(new Error("Cart.Empty", "No selected items were found in your cart."));

        var lines = new List<OrderLineInput>();
        foreach (var item in itemsToCheckout)
        {
            var product = await _sender.Send(new GetProductByIdQuery(item.ProductId), cancellationToken);
            if (product is null)
                return Result.Failure<Guid>(new Error(
                    "Product.NotFound", "A product in your cart no longer exists, please refresh your cart."));

            // Re-checked here (not just at add-to-cart time) since stock can move between then and
            // now — another buyer's order may have been approved in the meantime. Still only a
            // soft, buyer-facing check: the authoritative enforcement remains Order.Approve.
            if (item.Quantity > product.StockQuantity)
            {
                return Result.Failure<Guid>(new Error(
                    "Product.InsufficientStock",
                    $"Only {product.StockQuantity} of \"{product.Name}\" in stock — please update your cart."));
            }

            lines.Add(new OrderLineInput(product.Id, product.Name, item.Quantity, Money.Of(product.Price, product.Currency)));
        }

        Voucher? voucher = null;
        Money? discountAmount = null;
        if (!string.IsNullOrWhiteSpace(request.VoucherCode))
        {
            voucher = await _voucherRepository.GetByCodeAsync(request.VoucherCode, cancellationToken);
            if (voucher is null || !voucher.CanBeUsed(DateTime.UtcNow))
                return Result.Failure<Guid>(new Error("Voucher.Invalid", "This voucher code is invalid or has expired."));

            var subtotal = lines.Aggregate(
                Money.Zero(),
                (total, line) => total.Add(Money.Of(line.UnitPrice.Amount * line.Quantity, line.UnitPrice.Currency)));
            discountAmount = voucher.CalculateDiscount(subtotal);
        }

        var paymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod, ignoreCase: true);
        var shippingAddress = ShippingAddress.Of(request.RecipientName, request.PhoneNumber, request.AddressLine, request.City);
        var order = Order.Place(request.UserId, lines, paymentMethod, shippingAddress, discountAmount, voucher?.Code);

        voucher?.RecordUse();

        _orderRepository.Add(order);
        foreach (var item in itemsToCheckout)
            cart.RemoveItem(item.ProductId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}
