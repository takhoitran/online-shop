namespace OnlineShop.Modules.Catalog.Application.Abstractions;

/// <summary>
/// Infra concern behind an Application-level port so the storage backend can move from local
/// disk (dev) to a cloud blob store later without touching any Command/Handler.
/// </summary>
public interface IFileStorageService
{
    /// <returns>A URL the frontend/bot can load the file from.</returns>
    Task<string> SaveProductImageAsync(Guid productId, Stream content, string fileName, CancellationToken cancellationToken = default);

    /// <summary>Best-effort — a missing file is not an error (the DB row is always the source of
    /// truth; this only exists to stop orphaned files from accumulating on disk forever).</summary>
    Task DeleteAsync(string url, CancellationToken cancellationToken = default);
}
