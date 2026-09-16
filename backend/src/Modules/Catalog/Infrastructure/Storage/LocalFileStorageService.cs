using Microsoft.Extensions.Options;
using OnlineShop.Modules.Catalog.Application.Abstractions;

namespace OnlineShop.Modules.Catalog.Infrastructure.Storage;

/// <summary>
/// Dev-stage implementation — saves to local disk under the Api's wwwroot. Swapping to a cloud
/// blob store later means writing one new class behind IFileStorageService, nothing else changes.
/// </summary>
internal sealed class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveProductImageAsync(
        Guid productId, Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_options.RootPath);

        // Unique per call (not just per product) — a product can now have several images (the
        // primary cover photo plus a gallery), and each upload must land on its own file instead
        // of overwriting a previous one.
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{productId}_{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_options.RootPath, storedFileName);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        return $"{_options.PublicBaseUrl}/{storedFileName}";
    }

    public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        var prefix = _options.PublicBaseUrl + "/";
        if (!url.StartsWith(prefix, StringComparison.Ordinal))
            return Task.CompletedTask;

        var fileName = url[prefix.Length..];
        // Guard against a stray "../" ever escaping RootPath — this service only ever gets URLs it
        // generated itself, but there's no reason not to enforce it defensively.
        if (fileName != Path.GetFileName(fileName))
            return Task.CompletedTask;

        var fullPath = Path.Combine(_options.RootPath, fileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
