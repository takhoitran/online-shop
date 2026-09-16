namespace OnlineShop.Modules.Inventory.Application.Dtos;

public sealed record InventoryTransactionDto(
    Guid Id,
    string Type,
    int Quantity,
    Guid? OrderId,
    Guid PerformedByUserId,
    string? Note,
    DateTime OccurredAtUtc);
