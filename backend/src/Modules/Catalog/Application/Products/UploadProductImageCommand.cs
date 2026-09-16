using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Catalog.Application.Products;

/// <summary>Content is a stream so the Application layer never depends on ASP.NET's IFormFile.</summary>
public sealed record UploadProductImageCommand(Guid ProductId, Stream Content, string FileName) : IRequest<Result<string>>;
