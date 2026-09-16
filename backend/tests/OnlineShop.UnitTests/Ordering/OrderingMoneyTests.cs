using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Ordering.Domain.ValueObjects;

namespace OnlineShop.UnitTests.Ordering;

public class OrderingMoneyTests
{
    [Fact]
    public void Add_SumsAmountsOfSameCurrency()
    {
        var result = Money.Of(100_000).Add(Money.Of(50_000));

        Assert.Equal(150_000, result.Amount);
    }

    [Fact]
    public void Add_DifferentCurrencies_Throws()
    {
        Assert.Throws<DomainException>(() => Money.Of(100_000, "VND").Add(Money.Of(1, "USD")));
    }

    [Fact]
    public void Subtract_WithinBalance_ComputesDifference()
    {
        var result = Money.Of(100_000).Subtract(Money.Of(30_000));

        Assert.Equal(70_000, result.Amount);
    }

    [Fact]
    public void Subtract_LargerThanBalance_ClampsAtZero()
    {
        var result = Money.Of(30_000).Subtract(Money.Of(100_000));

        Assert.Equal(0, result.Amount);
    }

    [Fact]
    public void Subtract_DifferentCurrencies_Throws()
    {
        Assert.Throws<DomainException>(() => Money.Of(100_000, "VND").Subtract(Money.Of(1, "USD")));
    }
}
