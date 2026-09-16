namespace OnlineShop.Modules.Catalog.Application.Products;

/// <summary>
/// Checks the file's actual bytes (magic numbers), not just its declared extension — an
/// extension-only check (see the FluentValidation "allowed extensions" rule next to this) is
/// trivially bypassed by renaming any file to ".png". Shared by both the primary product-image
/// upload and the gallery-image upload, since both accept the same jpg/jpeg/png/webp formats.
/// </summary>
internal static class ImageContentValidator
{
    public static bool LooksLikeAnAllowedImage(Stream content)
    {
        if (!content.CanSeek)
            return false;

        var originalPosition = content.Position;
        try
        {
            content.Position = 0;
            Span<byte> header = stackalloc byte[12];
            var read = content.Read(header);
            if (read < 4)
                return false;

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                return true;

            // WEBP: "RIFF" .... "WEBP"
            if (read >= 12
                && header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
                && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P')
                return true;

            return false;
        }
        finally
        {
            content.Position = originalPosition;
        }
    }
}
