using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Inventory.Application.ReceiveStock;

/// <summary>PerformedByUserId comes from the authenticated JWT principal, not client input.</summary>
public sealed record ReceiveStockCommand(
    Guid ProductId,
    int Quantity,
    Guid PerformedByUserId,
    string? Note) : IRequest<Result>;
