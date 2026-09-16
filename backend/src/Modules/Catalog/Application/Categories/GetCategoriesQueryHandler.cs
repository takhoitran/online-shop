using MediatR;
using OnlineShop.Modules.Catalog.Application.Dtos;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.Modules.Catalog.Application.Categories;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        return categories
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.NameEn,
                c.NameVi,
                c.DescriptionEn,
                c.DescriptionVi))
            .ToList();
    }
}
