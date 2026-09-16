namespace OnlineShop.Modules.Catalog.Application.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    string? NameEn,
    string? NameVi,
    string? DescriptionEn,
    string? DescriptionVi);
