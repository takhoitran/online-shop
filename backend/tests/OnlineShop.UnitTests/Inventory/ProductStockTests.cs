using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Inventory.Domain;
using OnlineShop.Modules.Inventory.Domain.Events;

namespace OnlineShop.UnitTests.Inventory;

public class ProductStockTests
{
    private static ProductStock NewStock() => ProductStock.Initialize(Guid.NewGuid());

    [Fact]
    public void Initialize_StartsAtZero()
    {
        var stock = NewStock();
        Assert.Equal(0, stock.QuantityOnHand);
    }

    [Fact]
    public void ReceiveStock_IncreasesQuantityOnHand()
    {
        var stock = NewStock();

        stock.ReceiveStock(10, Guid.NewGuid(), "initial receipt");

        Assert.Equal(10, stock.QuantityOnHand);
    }

    [Fact]
    public void ReceiveStock_WithZeroQuantity_Throws()
    {
        var stock = NewStock();
        Assert.Throws<DomainException>(() => stock.ReceiveStock(0, Guid.NewGuid(), null));
    }

    [Fact]
    public void ReceiveStock_WithNegativeQuantity_Throws()
    {
        var stock = NewStock();
        Assert.ThrowsAny<Exception>(() => stock.ReceiveStock(-1, Guid.NewGuid(), null));
    }

    [Fact]
    public void IssueStock_NeverDropsBelowZero()
    {
        var stock = NewStock();
        stock.ReceiveStock(5, Guid.NewGuid(), null);

        var ex = Assert.Throws<DomainException>(() => stock.IssueStock(6, Guid.NewGuid(), null, "oversell attempt"));

        Assert.Contains("Exceeds", ex.Message);
        Assert.Equal(5, stock.QuantityOnHand);
    }

    [Fact]
    public void IssueStock_ExactlyAllOnHand_DropsToZero_NotNegative()
    {
        var stock = NewStock();
        stock.ReceiveStock(5, Guid.NewGuid(), null);

        stock.IssueStock(5, Guid.NewGuid(), null, "sold out");

        Assert.Equal(0, stock.QuantityOnHand);
    }

    [Fact]
    public void IssueStock_AtOrBelowThreshold_RaisesLowStockDetectedDomainEvent()
    {
        var stock = NewStock();
        stock.ReceiveStock(ProductStock.LowStockThreshold + 1, Guid.NewGuid(), null);
        stock.ClearDomainEvents();

        stock.IssueStock(1, Guid.NewGuid(), null, null);

        Assert.Contains(stock.DomainEvents, e => e is LowStockDetectedDomainEvent);
    }

    [Fact]
    public void IssueStock_AboveThreshold_DoesNotRaiseLowStockDetectedDomainEvent()
    {
        var stock = NewStock();
        stock.ReceiveStock(100, Guid.NewGuid(), null);
        stock.ClearDomainEvents();

        stock.IssueStock(1, Guid.NewGuid(), null, null);

        Assert.DoesNotContain(stock.DomainEvents, e => e is LowStockDetectedDomainEvent);
    }

    [Fact]
    public void ReturnStock_IncreasesQuantityOnHandAgain()
    {
        var stock = NewStock();
        stock.ReceiveStock(10, Guid.NewGuid(), null);
        stock.IssueStock(4, Guid.NewGuid(), Guid.NewGuid(), null);

        stock.ReturnStock(4, Guid.NewGuid(), Guid.NewGuid(), "order cancelled after approval");

        Assert.Equal(10, stock.QuantityOnHand);
    }

    [Fact]
    public void IssueStock_WithZeroQuantity_Throws()
    {
        var stock = NewStock();
        stock.ReceiveStock(10, Guid.NewGuid(), null);

        Assert.Throws<DomainException>(() => stock.IssueStock(0, Guid.NewGuid(), null, null));
    }
}
