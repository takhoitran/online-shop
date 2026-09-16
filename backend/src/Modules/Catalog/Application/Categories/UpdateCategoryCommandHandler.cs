using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Categories;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, ICatalogUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure(new Error("Category.NotFound", "Category not found."));

        category.Rename(
            request.Name,
            request.Description,
            request.NameEn,
            request.NameVi,
            request.DescriptionEn,
            request.DescriptionVi);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
