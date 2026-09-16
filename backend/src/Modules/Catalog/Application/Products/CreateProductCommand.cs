using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    Guid CategoryId,
    string? NameEn = null,
    string? NameVi = null,
    string? DescriptionEn = null,
    string? DescriptionVi = null) : IRequest<Result<Guid>>;
