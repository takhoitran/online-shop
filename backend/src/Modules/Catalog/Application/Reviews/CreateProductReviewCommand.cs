using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Catalog.Application.Reviews;

public sealed record CreateProductReviewCommand(
    Guid ProductId,
    Guid BuyerId,
    string BuyerName,
    int Rating,
    string? Comment) : IRequest<Result<Guid>>;
