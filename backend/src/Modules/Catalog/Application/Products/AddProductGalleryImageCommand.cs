using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed record AddProductGalleryImageCommand(Guid ProductId, Stream Content, string FileName) : IRequest<Result<string>>;
