using FluentValidation;

namespace OnlineShop.Modules.Catalog.Application.Products;

public sealed class UploadProductImageCommandValidator : AbstractValidator<UploadProductImageCommand>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public UploadProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(name => AllowedExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
            .WithMessage("Only jpg, jpeg, png, or webp image formats are accepted.");
        RuleFor(x => x.Content)
            .Must(ImageContentValidator.LooksLikeAnAllowedImage)
            .WithMessage("The file's content doesn't match a jpg, jpeg, png, or webp image.");
    }
}
