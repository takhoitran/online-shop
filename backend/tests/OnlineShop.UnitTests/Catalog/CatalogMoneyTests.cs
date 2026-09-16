using OnlineShop.BuildingBlocks.Domain;
using CatalogMoney = OnlineShop.Modules.Catalog.Domain.ValueObjects.Money;

namespace OnlineShop.UnitTests.Catalog;

public class CatalogMoneyTests
{
    [Fact]
    public void Of_WithPositiveAmount_Succeeds()
    {
        var money = CatalogMoney.Of(100_000);
        Assert.Equal(100_000, money.Amount);
        Assert.Equal("VND", money.Currency);
    }

    [Fact]
    public void Of_WithZero_Throws()
    {
        Assert.Throws<DomainException>(() => CatalogMoney.Of(0));
    }

    [Fact]
    public void Of_WithNegative_Throws()
    {
        Assert.Throws<DomainException>(() => CatalogMoney.Of(-1));
    }

    [Fact]
    public void Of_NormalizesCurrencyToUppercase()
    {
        var money = CatalogMoney.Of(1000, "vnd");
        Assert.Equal("VND", money.Currency);
    }
}
