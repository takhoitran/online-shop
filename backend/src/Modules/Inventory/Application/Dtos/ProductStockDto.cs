namespace OnlineShop.Modules.Inventory.Application.Dtos;

public sealed record ProductStockDto(
    Guid ProductId,
    int QuantityOnHand,
    IReadOnlyList<InventoryTransactionDto> RecentTransactions);
