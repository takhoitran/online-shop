using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed record RemoveProductGalleryImageCommand(Guid ProductId, Guid ImageId) : IRequest<Result>;
