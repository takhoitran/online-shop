using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Inventory.Domain.Events;

namespace OnlineShop.Modules.Inventory.Domain;

/// <summary>
/// The single source of truth for "how much of this product do we have". Deliberately its own
/// Aggregate, separate from Catalog's Product — editing a product's description and receiving a
/// shipment are two unrelated transactions and must never contend for the same lock. Reuses the
/// Product's own id as its own Id: there is exactly one stock record per product, so a second
/// identity would only be indirection with no invariant it protects.
/// </summary>
public sealed class ProductStock : AggregateRoot<Guid>
{
    /// <summary>Below this, a LowStockDetectedDomainEvent is raised after an issue.</summary>
    public const int LowStockThreshold = 5;

    private readonly List<InventoryTransaction> _transactions = new();

    public Guid ProductId => Id;
    public int QuantityOnHand { get; private set; }
    public IReadOnlyCollection<InventoryTransaction> Transactions => _transactions.AsReadOnly();

    private ProductStock() { }

    private ProductStock(Guid productId) : base(productId)
    {
        QuantityOnHand = 0;
    }

    /// <summary>Called by the handler reacting to Catalog's ProductCreatedDomainEvent — a brand
    /// new product always starts with zero stock until someone receives stock for it.</summary>
    public static ProductStock Initialize(Guid productId) => new(productId);

    public void ReceiveStock(int quantity, Guid performedByUserId, string? note)
    {
        Guard.AgainstNegative(quantity, nameof(quantity));
        if (quantity == 0)
            throw new DomainException("Received quantity must be greater than 0.");

        QuantityOnHand += quantity;
        _transactions.Add(InventoryTransaction.In(quantity, performedByUserId, note));

        Raise(new StockReceivedDomainEvent(ProductId, QuantityOnHand));
    }

    /// <summary>orderId is set when Ordering (Phase 4) triggers this on approval; left null for a
    /// manual Seller adjustment (write-off, damaged goods, stocktake correction).</summary>
    public void IssueStock(int quantity, Guid performedByUserId, Guid? orderId, string? note)
    {
        Guard.AgainstNegative(quantity, nameof(quantity));
        if (quantity == 0)
            throw new DomainException("Issued quantity must be greater than 0.");

        if (QuantityOnHand - quantity < 0)
            throw new DomainException("Exceeds the current stock on hand.");

        QuantityOnHand -= quantity;
        _transactions.Add(InventoryTransaction.Out(quantity, performedByUserId, orderId, note));

        Raise(new StockIssuedDomainEvent(ProductId, QuantityOnHand, orderId));

        if (QuantityOnHand <= LowStockThreshold)
            Raise(new LowStockDetectedDomainEvent(ProductId, QuantityOnHand));
    }

    /// <summary>An approved order gets cancelled/returned after stock was already issued for it.</summary>
    public void ReturnStock(int quantity, Guid performedByUserId, Guid orderId, string? note)
    {
        Guard.AgainstNegative(quantity, nameof(quantity));
        if (quantity == 0)
            throw new DomainException("Returned quantity must be greater than 0.");

        QuantityOnHand += quantity;
        _transactions.Add(InventoryTransaction.Return(quantity, performedByUserId, orderId, note));

        Raise(new StockReturnedDomainEvent(ProductId, QuantityOnHand, orderId));
    }
}
