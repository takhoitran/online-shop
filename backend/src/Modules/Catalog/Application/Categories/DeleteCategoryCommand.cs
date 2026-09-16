using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Catalog.Application.Categories;

public sealed record DeleteCategoryCommand(Guid CategoryId) : IRequest<Result>;
