using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Application.Categories;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description,
    string? NameEn = null,
    string? NameVi = null,
    string? DescriptionEn = null,
    string? DescriptionVi = null) : IRequest<Result<CategoryDto>>;
