using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Catalog.Application.Categories;

public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    string? NameEn = null,
    string? NameVi = null,
    string? DescriptionEn = null,
    string? DescriptionVi = null) : IRequest<Result>;
