using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Ordering.Domain;
using OnlineShop.Modules.Ordering.Domain.ValueObjects;

namespace OnlineShop.UnitTests.Ordering;

public class VoucherTests
{
    [Fact]
    public void Create_WithZeroDiscountValue_Throws()
    {
        Assert.Throws<DomainException>(() => Voucher.Create("CODE", DiscountType.FixedAmount, 0, null, null));
    }

    [Fact]
    public void Create_PercentageAbove100_Throws()
    {
        Assert.Throws<DomainException>(() => Voucher.Create("CODE", DiscountType.Percentage, 101, null, null));
    }

    [Fact]
    public void Create_WithZeroMaxUses_Throws()
    {
        Assert.Throws<DomainException>(() => Voucher.Create("CODE", DiscountType.FixedAmount, 10_000, 0, null));
    }

    [Fact]
    public void Create_NormalizesCodeToUppercaseAndTrimmed()
    {
        var voucher = Voucher.Create("  code10  ", DiscountType.Percentage, 10, null, null);
        Assert.Equal("CODE10", voucher.Code);
    }

    [Fact]
    public void CalculateDiscount_Percentage_ComputesPercentOfSubtotal()
    {
        var voucher = Voucher.Create("PCT10", DiscountType.Percentage, 10, null, null);

        var discount = voucher.CalculateDiscount(Money.Of(200_000));

        Assert.Equal(20_000, discount.Amount);
    }

    [Fact]
    public void CalculateDiscount_FixedAmount_NeverExceedsSubtotal()
    {
        var voucher = Voucher.Create("FLAT50K", DiscountType.FixedAmount, 50_000, null, null);

        var discount = voucher.CalculateDiscount(Money.Of(30_000));

        Assert.Equal(30_000, discount.Amount);
    }

    [Fact]
    public void CanBeUsed_WhenDeactivated_ReturnsFalse()
    {
        var voucher = Voucher.Create("CODE", DiscountType.Percentage, 10, null, null);
        voucher.Deactivate();

        Assert.False(voucher.CanBeUsed(DateTime.UtcNow));
    }

    [Fact]
    public void CanBeUsed_AfterExpiry_ReturnsFalse()
    {
        var voucher = Voucher.Create("CODE", DiscountType.Percentage, 10, null, DateTime.UtcNow.AddDays(-1));

        Assert.False(voucher.CanBeUsed(DateTime.UtcNow));
    }

    [Fact]
    public void CanBeUsed_AfterMaxUsesReached_ReturnsFalse()
    {
        var voucher = Voucher.Create("CODE", DiscountType.Percentage, 10, maxUses: 1, null);
        voucher.RecordUse();

        Assert.False(voucher.CanBeUsed(DateTime.UtcNow));
    }

    [Fact]
    public void RecordUse_IncrementsUsedCount()
    {
        var voucher = Voucher.Create("CODE", DiscountType.Percentage, 10, maxUses: 2, null);

        voucher.RecordUse();

        Assert.Equal(1, voucher.UsedCount);
    }

    [Fact]
    public void RecordUse_WhenExhausted_Throws()
    {
        var voucher = Voucher.Create("CODE", DiscountType.Percentage, 10, maxUses: 1, null);
        voucher.RecordUse();

        Assert.Throws<DomainException>(() => voucher.RecordUse());
    }

    [Fact]
    public void ReleaseUse_DecrementsUsedCount()
    {
        var voucher = Voucher.Create("CODE", DiscountType.Percentage, 10, maxUses: 2, null);
        voucher.RecordUse();

        voucher.ReleaseUse();

        Assert.Equal(0, voucher.UsedCount);
    }

    [Fact]
    public void ReleaseUse_WhenAlreadyZero_StaysAtZero()
    {
        var voucher = Voucher.Create("CODE", DiscountType.Percentage, 10, null, null);

        voucher.ReleaseUse();

        Assert.Equal(0, voucher.UsedCount);
    }

    [Fact]
    public void ReleaseUse_MakesAnExhaustedVoucherUsableAgain()
    {
        var voucher = Voucher.Create("CODE", DiscountType.Percentage, 10, maxUses: 1, null);
        voucher.RecordUse();

        voucher.ReleaseUse();

        Assert.True(voucher.CanBeUsed(DateTime.UtcNow));
    }

    [Fact]
    public void UpdateDetails_ChangesDiscountValueAndExpiry()
    {
        var voucher = Voucher.Create("CODE", DiscountType.Percentage, 10, null, null);
        var newExpiry = DateTime.UtcNow.AddDays(30);

        voucher.UpdateDetails(DiscountType.FixedAmount, 25_000, 5, newExpiry);

        Assert.Equal(DiscountType.FixedAmount, voucher.DiscountType);
        Assert.Equal(25_000, voucher.DiscountValue);
        Assert.Equal(5, voucher.MaxUses);
        Assert.Equal(newExpiry, voucher.ExpiresAtUtc);
    }

    [Fact]
    public void UpdateDetails_MaxUsesBelowAlreadyUsedCount_Throws()
    {
        var voucher = Voucher.Create("CODE", DiscountType.Percentage, 10, maxUses: 5, null);
        voucher.RecordUse();
        voucher.RecordUse();

        Assert.Throws<DomainException>(() => voucher.UpdateDetails(DiscountType.Percentage, 10, maxUses: 1, null));
    }

    [Fact]
    public void UpdateDetails_DoesNotChangeCode()
    {
        var voucher = Voucher.Create("ORIGINAL", DiscountType.Percentage, 10, null, null);

        voucher.UpdateDetails(DiscountType.Percentage, 20, null, null);

        Assert.Equal("ORIGINAL", voucher.Code);
    }
}
