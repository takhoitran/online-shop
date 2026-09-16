using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Inventory.Domain;

/// <summary>
/// One append-only ledger row per stock movement — never edited or deleted, so the total stock
/// can always be audited/reconciled from history instead of trusting a single mutable counter.
/// </summary>
public sealed class InventoryTransaction : Entity<Guid>
{
    public InventoryTransactionType Type { get; private set; }
    public int Quantity { get; private set; }
    public Guid? OrderId { get; private set; }
    public Guid PerformedByUserId { get; private set; }
    public string? Note { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private InventoryTransaction() { }

    private InventoryTransaction(
        Guid id,
        InventoryTransactionType type,
        int quantity,
        Guid performedByUserId,
        Guid? orderId,
        string? note) : base(id)
    {
        Type = type;
        Quantity = quantity;
        PerformedByUserId = performedByUserId;
        OrderId = orderId;
        Note = note;
        OccurredAtUtc = DateTime.UtcNow;
    }

    public static InventoryTransaction In(int quantity, Guid performedByUserId, string? note) =>
        new(Guid.NewGuid(), InventoryTransactionType.In, quantity, performedByUserId, orderId: null, note);

    public static InventoryTransaction Out(int quantity, Guid performedByUserId, Guid? orderId, string? note) =>
        new(Guid.NewGuid(), InventoryTransactionType.Out, quantity, performedByUserId, orderId, note);

    public static InventoryTransaction Return(int quantity, Guid performedByUserId, Guid orderId, string? note) =>
        new(Guid.NewGuid(), InventoryTransactionType.Return, quantity, performedByUserId, orderId, note);
}
