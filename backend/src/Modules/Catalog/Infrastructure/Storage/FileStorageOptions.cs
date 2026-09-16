namespace OnlineShop.Modules.Catalog.Infrastructure.Storage;

public sealed class FileStorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Absolute or relative-to-Api-content-root folder to write uploaded files into.</summary>
    public string RootPath { get; set; } = "wwwroot/uploads/products";

    /// <summary>Public URL prefix the frontend/bot uses to load a saved file back.</summary>
    public string PublicBaseUrl { get; set; } = "/uploads/products";
}
