using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Ordering.Domain.ValueObjects;

namespace OnlineShop.Modules.Ordering.Domain;

/// <summary>
/// Its own Aggregate Root — a promo code has a lifecycle (active/expired/exhausted) entirely
/// independent of any single Order. Applying one at checkout mutates both this aggregate (usage
/// count) and the new Order in the same transaction; not textbook-strict DDD (one aggregate per
/// transaction), but the same compromise this codebase already makes in CheckoutCommandHandler,
/// which saves Cart.Clear() and the new Order together.
/// </summary>
public sealed class Voucher : AggregateRoot<Guid>
{
    public string Code { get; private set; } = default!;
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public int? MaxUses { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Voucher() { }

    private Voucher(Guid id, string code, DiscountType discountType, decimal discountValue, int? maxUses, DateTime? expiresAtUtc)
        : base(id)
    {
        Code = code;
        DiscountType = discountType;
        DiscountValue = discountValue;
        MaxUses = maxUses;
        ExpiresAtUtc = expiresAtUtc;
        IsActive = true;
        UsedCount = 0;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Voucher Create(string code, DiscountType discountType, decimal discountValue, int? maxUses, DateTime? expiresAtUtc)
    {
        Guard.AgainstNullOrWhiteSpace(code, nameof(code));
        if (discountValue <= 0)
            throw new DomainException("Discount value must be greater than 0.");
        if (discountType == DiscountType.Percentage && discountValue > 100)
            throw new DomainException("A percentage discount cannot exceed 100.");
        if (maxUses is <= 0)
            throw new DomainException("Max uses must be greater than 0 when specified.");

        return new Voucher(Guid.NewGuid(), code.Trim().ToUpperInvariant(), discountType, discountValue, maxUses, expiresAtUtc);
    }

    public void Deactivate() => IsActive = false;

    /// <summary>Code is deliberately not editable — buyers may already know/have shared it;
    /// changing everything else (a typo in the discount value, a wrong expiry date, etc.) doesn't
    /// require burning the code and losing its usage history the way Deactivate-then-Create would.</summary>
    public void UpdateDetails(DiscountType discountType, decimal discountValue, int? maxUses, DateTime? expiresAtUtc)
    {
        if (discountValue <= 0)
            throw new DomainException("Discount value must be greater than 0.");
        if (discountType == DiscountType.Percentage && discountValue > 100)
            throw new DomainException("A percentage discount cannot exceed 100.");
        if (maxUses is <= 0)
            throw new DomainException("Max uses must be greater than 0 when specified.");
        if (maxUses.HasValue && maxUses.Value < UsedCount)
            throw new DomainException($"Max uses cannot be set below the {UsedCount} time(s) this voucher has already been used.");

        DiscountType = discountType;
        DiscountValue = discountValue;
        MaxUses = maxUses;
        ExpiresAtUtc = expiresAtUtc;
    }

    /// <summary>Called when an order that consumed this voucher is Cancelled or Returned — the
    /// discount was never actually redeemed, so the use shouldn't count against a limited code.</summary>
    public void ReleaseUse()
    {
        if (UsedCount > 0)
            UsedCount--;
    }

    /// <summary>The Application layer calls this only after CalculateDiscount succeeded — the two
    /// are split so Checkout can preview a discount without consuming a use.</summary>
    public bool CanBeUsed(DateTime nowUtc) =>
        IsActive
        && (ExpiresAtUtc is null || ExpiresAtUtc >= nowUtc)
        && (MaxUses is null || UsedCount < MaxUses);

    public Money CalculateDiscount(Money subtotal) =>
        DiscountType == DiscountType.Percentage
            ? Money.Of(Math.Round(subtotal.Amount * DiscountValue / 100m, 2), subtotal.Currency)
            : Money.Of(Math.Min(DiscountValue, subtotal.Amount), subtotal.Currency);

    public void RecordUse()
    {
        if (!CanBeUsed(DateTime.UtcNow))
            throw new DomainException("This voucher can no longer be used.");

        UsedCount++;
    }
}
