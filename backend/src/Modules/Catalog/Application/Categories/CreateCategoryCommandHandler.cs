using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Abstractions;
using OnlineShop.Modules.Catalog.Application.Dtos;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Categories;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, ICatalogUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(
            request.Name,
            request.Description,
            request.NameEn,
            request.NameVi,
            request.DescriptionEn,
            request.DescriptionVi);

        _categoryRepository.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.NameEn,
            category.NameVi,
            category.DescriptionEn,
            category.DescriptionVi));
    }
}
