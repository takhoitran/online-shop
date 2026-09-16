using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Catalog.Domain;

public sealed class Category : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? NameEn { get; private set; }
    public string? NameVi { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string? DescriptionVi { get; private set; }

    private Category() { }

    private Category(
        Guid id,
        string name,
        string? description,
        string? nameEn,
        string? nameVi,
        string? descriptionEn,
        string? descriptionVi) : base(id)
    {
        Name = name;
        Description = description;
        NameEn = nameEn;
        NameVi = nameVi;
        DescriptionEn = descriptionEn;
        DescriptionVi = descriptionVi;
    }

    public static Category Create(
        string name,
        string? description,
        string? nameEn = null,
        string? nameVi = null,
        string? descriptionEn = null,
        string? descriptionVi = null)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        return new Category(
            Guid.NewGuid(),
            name.Trim(),
            NormalizeOptional(description),
            NormalizeOptional(nameEn),
            NormalizeOptional(nameVi),
            NormalizeOptional(descriptionEn),
            NormalizeOptional(descriptionVi));
    }

    public void Rename(
        string name,
        string? description,
        string? nameEn = null,
        string? nameVi = null,
        string? descriptionEn = null,
        string? descriptionVi = null)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Name = name.Trim();
        Description = NormalizeOptional(description);
        NameEn = NormalizeOptional(nameEn);
        NameVi = NormalizeOptional(nameVi);
        DescriptionEn = NormalizeOptional(descriptionEn);
        DescriptionVi = NormalizeOptional(descriptionVi);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
