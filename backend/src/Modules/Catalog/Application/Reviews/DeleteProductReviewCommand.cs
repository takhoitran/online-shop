using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Catalog.Application.Reviews;

/// <summary>Seller/Admin moderation only — a Buyer cannot delete their own review once posted
/// (matches most real storefronts: a review is a record of what was said, not freely editable).</summary>
public sealed record DeleteProductReviewCommand(Guid ReviewId) : IRequest<Result>;
