namespace IngredientServer.Core.Configuration;

/// <summary>
/// Configuration options for file upload
/// </summary>
public class FileUploadOptions
{
    public const string SectionName = "FileUpload";

    /// <summary>
    /// Maximum file size in bytes (default: 10MB)
    /// </summary>
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Allowed file extensions (comma-separated)
    /// </summary>
    public string AllowedExtensions { get; set; } = ".jpg,.jpeg,.png,.gif,.webp";

    /// <summary>
    /// Get allowed extensions as array
    /// </summary>
    public string[] GetAllowedExtensionsArray()
    {
        return AllowedExtensions
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLowerInvariant())
            .ToArray();
    }
}

