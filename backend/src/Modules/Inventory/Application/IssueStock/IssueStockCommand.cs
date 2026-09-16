using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Inventory.Application.IssueStock;

/// <summary>
/// Manual stock write-off (damaged goods, stocktake correction) — OrderId is always null here.
/// Order-driven issuance (Phase 4) calls ProductStock.IssueStock(...) directly from Ordering's
/// OrderApproved event handler, not through this command.
/// </summary>
public sealed record IssueStockCommand(
    Guid ProductId,
    int Quantity,
    Guid PerformedByUserId,
    string? Note) : IRequest<Result>;
