using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.Catalog.Application.Categories;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetCategoriesQuery(), cancellationToken));

    [Authorize(Roles = "Seller,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken cancellationToken) =>
        (await _sender.Send(command, cancellationToken)).ToActionResult();

    [Authorize(Roles = "Seller,Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.Name,
            request.Description,
            request.NameEn,
            request.NameVi,
            request.DescriptionEn,
            request.DescriptionVi);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    [Authorize(Roles = "Seller,Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        (await _sender.Send(new DeleteCategoryCommand(id), cancellationToken)).ToActionResult();
}

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    string? NameEn = null,
    string? NameVi = null,
    string? DescriptionEn = null,
    string? DescriptionVi = null);
