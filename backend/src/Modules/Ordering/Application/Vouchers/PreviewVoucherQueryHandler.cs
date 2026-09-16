using MediatR;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Ordering.Application.Dtos;
using OnlineShop.Modules.Ordering.Domain;
using OnlineShop.Modules.Ordering.Domain.ValueObjects;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed class PreviewVoucherQueryHandler : IRequestHandler<PreviewVoucherQuery, VoucherPreviewDto>
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ISender _sender;

    public PreviewVoucherQueryHandler(IVoucherRepository voucherRepository, ICartRepository cartRepository, ISender sender)
    {
        _voucherRepository = voucherRepository;
        _cartRepository = cartRepository;
        _sender = sender;
    }

    public async Task<VoucherPreviewDto> Handle(PreviewVoucherQuery request, CancellationToken cancellationToken)
    {
        var voucher = await _voucherRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (voucher is null || !voucher.CanBeUsed(DateTime.UtcNow))
            return new VoucherPreviewDto(false, "This voucher code is invalid or has expired.", 0, 0, Money.DefaultCurrency);

        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
            return new VoucherPreviewDto(false, "Your cart is empty.", 0, 0, Money.DefaultCurrency);

        // Mirror CheckoutCommandHandler's selection filtering so the previewed discount matches
        // what actually gets applied when only some cart items are checked out.
        var itemsToPreview = request.ProductIds is { Count: > 0 } selected
            ? cart.Items.Where(i => selected.Contains(i.ProductId)).ToList()
            : cart.Items.ToList();
        if (itemsToPreview.Count == 0)
            return new VoucherPreviewDto(false, "No selected items were found in your cart.", 0, 0, Money.DefaultCurrency);

        var subtotal = Money.Zero();
        foreach (var item in itemsToPreview)
        {
            var product = await _sender.Send(new GetProductByIdQuery(item.ProductId), cancellationToken);
            if (product is null) continue;
            subtotal = subtotal.Add(Money.Of(product.Price * item.Quantity, product.Currency));
        }

        var discount = voucher.CalculateDiscount(subtotal);
        var newTotal = subtotal.Subtract(discount);

        return new VoucherPreviewDto(true, null, discount.Amount, newTotal.Amount, newTotal.Currency);
    }
}
