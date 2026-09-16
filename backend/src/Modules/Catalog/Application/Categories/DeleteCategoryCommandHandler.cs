using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Categories;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductCatalogQueries _productCatalogQueries;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IProductCatalogQueries productCatalogQueries,
        ICatalogUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _productCatalogQueries = productCatalogQueries;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure(new Error("Category.NotFound", "Category not found."));

        var productsInCategory = await _productCatalogQueries.SearchAsync(
            new ProductSearchFilter(null, request.CategoryId, null, null, 1, 1), cancellationToken);
        if (productsInCategory.TotalCount > 0)
            return Result.Failure(new Error("Category.HasProducts", "Cannot delete a category that still has products in it. Move or delete those products first."));

        _categoryRepository.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
