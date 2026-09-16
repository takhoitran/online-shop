using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Catalog.Application.Reviews;
using OnlineShop.Modules.Ordering.Application.Orders;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    /// <summary>Shared search/list endpoint — used by the Web storefront, the Telegram bot, and
    /// (in-process, via the same GetProductsQuery) AI Advisory once Phase 8 lands. SoldCount is
    /// composed in here (not by Catalog itself): Catalog can never depend on Ordering.Application
    /// (Ordering.Application already depends on Catalog.Application — the reverse would cycle),
    /// so "how many units sold" can only be merged at the one layer allowed to touch every
    /// module — the Api/Presentation layer.</summary>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] Guid? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = null,
        [FromQuery] double? minRating = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(keyword, categoryId, minPrice, maxPrice, page, pageSize, sort, minRating);
        var result = await _sender.Send(query, cancellationToken);
        var soldCountByProductId = await GetSoldCountByProductIdAsync(cancellationToken);

        var items = result.Items.Select(p => new
        {
            p.Id, p.Name, p.NameEn, p.NameVi, p.Price, p.Currency, p.ImageUrl, p.StockQuantity,
            p.CategoryId, p.CategoryName, p.CategoryNameEn, p.CategoryNameVi,
            p.AverageRating, p.ReviewCount,
            SoldCount = soldCountByProductId.GetValueOrDefault(p.Id, 0),
        });
        return Ok(new { items, result.Page, result.PageSize, result.TotalCount, result.TotalPages });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductByIdQuery(id), cancellationToken);
        if (product is null) return NotFound();

        var soldCountByProductId = await GetSoldCountByProductIdAsync(cancellationToken);
        return Ok(new
        {
            product.Id, product.Name, product.Description, product.NameEn, product.NameVi,
            product.DescriptionEn, product.DescriptionVi, product.Price, product.Currency, product.ImageUrl,
            product.StockQuantity, product.CategoryId, product.CategoryName, product.CategoryNameEn, product.CategoryNameVi,
            product.CreatedAtUtc, product.UpdatedAtUtc,
            product.AverageRating, product.ReviewCount, product.Images,
            SoldCount = soldCountByProductId.GetValueOrDefault(product.Id, 0),
        });
    }

    private async Task<Dictionary<Guid, int>> GetSoldCountByProductIdAsync(CancellationToken cancellationToken)
    {
        var salesStats = await _sender.Send(new GetProductSalesStatsQuery(500), cancellationToken);
        return salesStats.ToDictionary(s => s.ProductId, s => s.TotalQuantitySold);
    }

    [Authorize(Roles = "Seller,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken cancellationToken) =>
        (await _sender.Send(command, cancellationToken)).ToActionResult();

    [Authorize(Roles = "Seller,Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            request.NameEn,
            request.NameVi,
            request.DescriptionEn,
            request.DescriptionVi);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    [Authorize(Roles = "Seller,Admin")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [HttpPost("{id:guid}/image")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return BadRequest("The image file is empty.");

        await using var stream = file.OpenReadStream();
        var command = new UploadProductImageCommand(id, stream, file.FileName);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    [Authorize(Roles = "Seller,Admin")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> AddGalleryImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return BadRequest("The image file is empty.");

        await using var stream = file.OpenReadStream();
        var command = new AddProductGalleryImageCommand(id, stream, file.FileName);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    [Authorize(Roles = "Seller,Admin")]
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> RemoveGalleryImage(Guid id, Guid imageId, CancellationToken cancellationToken) =>
        (await _sender.Send(new RemoveProductGalleryImageCommand(id, imageId), cancellationToken)).ToActionResult();

    [Authorize(Roles = "Seller,Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        (await _sender.Send(new DeleteProductCommand(id), cancellationToken)).ToActionResult();

    [HttpGet("{id:guid}/reviews")]
    public async Task<IActionResult> GetReviews(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetProductReviewsQuery(id, page, pageSize);
        return Ok(await _sender.Send(query, cancellationToken));
    }

    [Authorize(Roles = "Buyer")]
    [HttpPost("{id:guid}/reviews")]
    public async Task<IActionResult> CreateReview(Guid id, CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var buyerName = User.FindFirstValue(ClaimTypes.Name) ?? "Buyer";
        var command = new CreateProductReviewCommand(id, CurrentUserId, buyerName, request.Rating, request.Comment);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    /// <summary>Moderation — Seller/Admin can remove a review's content (e.g. spam, abuse); a
    /// Buyer cannot delete their own review once posted.</summary>
    [Authorize(Roles = "Seller,Admin")]
    [HttpDelete("{id:guid}/reviews/{reviewId:guid}")]
    public async Task<IActionResult> DeleteReview(Guid id, Guid reviewId, CancellationToken cancellationToken) =>
        (await _sender.Send(new DeleteProductReviewCommand(reviewId), cancellationToken)).ToActionResult();
}

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    Guid CategoryId,
    string? NameEn = null,
    string? NameVi = null,
    string? DescriptionEn = null,
    string? DescriptionVi = null);
public sealed record CreateReviewRequest(int Rating, string? Comment);
