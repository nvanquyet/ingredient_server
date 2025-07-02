using IngredientServer.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace IngredientServer.Core.Services;

public class ImageService(IHttpContextAccessor httpContextAccessor) : IImageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public async Task<string?> SaveImageAsync(IFormFile? image)
    {
        if (image == null || image.Length == 0)
            return null;

        ValidateImage(image);

        try
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(fileStream);

            return GenerateImageUrl(uniqueFileName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save image.", ex);
        }
    }

    public async Task<string?> UpdateImageAsync(IFormFile? newImage, string? existingImageUrl)
    {
        // Nếu không có ảnh mới, giữ nguyên ảnh cũ
        if (newImage == null || newImage.Length == 0)
            return existingImageUrl;

        try
        {
            // Xóa ảnh cũ trước (nếu có)
            await DeleteImageAsync(existingImageUrl);

            // Lưu ảnh mới
            return await SaveImageAsync(newImage);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to update image.", ex);
        }
    }

    public async Task DeleteImageAsync(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return;

        try
        {
            var fileName = ExtractFileNameFromUrl(imageUrl);
            if (string.IsNullOrEmpty(fileName))
                return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
        catch (Exception)
        {
            // Log error nhưng không throw exception
            // Việc không xóa được file cũ không nên làm fail toàn bộ operation
            // _logger?.LogWarning("Failed to delete image: {ImageUrl}", imageUrl);
        }
    }

    private void ValidateImage(IFormFile image)
    {
        if (image.Length > MaxFileSize)
            throw new InvalidOperationException("File size exceeds 5MB limit.");

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Invalid file type. Only images are allowed.");

        if (!AllowedMimeTypes.Contains(image.ContentType.ToLowerInvariant()))
            throw new InvalidOperationException("Invalid file format.");
    }

    private string GenerateImageUrl(string fileName)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HTTP context is not available.");

        return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/uploads/{fileName}";
    }

    private static string? ExtractFileNameFromUrl(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return null;

        try
        {
            // Extract filename from URL like "https://localhost:5001/uploads/filename.jpg"
            var uri = new Uri(imageUrl);
            return Path.GetFileName(uri.LocalPath);
        }
        catch
        {
            // Fallback: try to extract filename directly
            return Path.GetFileName(imageUrl);
        }
    }
}