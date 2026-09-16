using MediatR;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Application.Categories;

public sealed record GetCategoriesQuery : IRequest<List<CategoryDto>>;
