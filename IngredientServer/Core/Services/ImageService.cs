using IngredientServer.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IngredientServer.Core.Services;

public class ImageService(
    IHttpContextAccessor httpContextAccessor,
    ILogger<ImageService> logger) : IImageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public async Task<string?> SaveImageAsync(IFormFile? image)
    {
        logger.LogInformation("=== START IMAGE SAVE OPERATION ===");
        
        if (image == null || image.Length == 0)
        {
            logger.LogInformation("No image provided or empty image");
            return null;
        }

        logger.LogInformation("Processing image - FileName: {FileName}, Size: {Size} bytes, ContentType: {ContentType}", 
            image.FileName, image.Length, image.ContentType);

        try
        {
            ValidateImage(image);
            logger.LogInformation("Image validation passed");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            
            if (!Directory.Exists(uploadsFolder))
            {
                logger.LogInformation("Creating uploads directory: {UploadsFolder}", uploadsFolder);
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            logger.LogInformation("Saving image to: {FilePath}", filePath);

            var startTime = DateTime.UtcNow;
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(fileStream);
            var saveTime = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;

            logger.LogInformation("Image saved successfully in {SaveTime}ms", saveTime);

            var imageUrl = GenerateImageUrl(uniqueFileName);
            logger.LogInformation("Generated image URL: {ImageUrl}", imageUrl);
            
            return imageUrl;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Image validation failed for file: {FileName}", image.FileName);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save image: {FileName}", image.FileName);
            throw new InvalidOperationException("Failed to save image.", ex);
        }
    }

    public async Task<string?> UpdateImageAsync(IFormFile? newImage, string? existingImageUrl)
    {
        logger.LogInformation("=== START IMAGE UPDATE OPERATION ===");
        logger.LogInformation("Existing image URL: {ExistingUrl}", existingImageUrl ?? "None");
        
        // Nếu không có ảnh mới, giữ nguyên ảnh cũ
        if (newImage == null || newImage.Length == 0)
        {
            logger.LogInformation("No new image provided, keeping existing image");
            return existingImageUrl;
        }

        logger.LogInformation("New image provided - FileName: {FileName}, Size: {Size} bytes", 
            newImage.FileName, newImage.Length);

        try
        {
            // Xóa ảnh cũ trước (nếu có)
            if (!string.IsNullOrEmpty(existingImageUrl))
            {
                logger.LogInformation("Deleting old image before saving new one");
                await DeleteImageAsync(existingImageUrl);
            }

            // Lưu ảnh mới
            var newImageUrl = await SaveImageAsync(newImage);
            logger.LogInformation("Image update completed successfully. New URL: {NewUrl}", newImageUrl);
            
            return newImageUrl;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update image from {OldUrl} to new image {NewFileName}", 
                existingImageUrl, newImage.FileName);
            throw new InvalidOperationException("Failed to update image.", ex);
        }
    }

    public async Task DeleteImageAsync(string? imageUrl)
    {
        logger.LogInformation("=== START IMAGE DELETE OPERATION ===");
        
        if (string.IsNullOrEmpty(imageUrl))
        {
            logger.LogInformation("No image URL provided for deletion");
            return;
        }

        logger.LogInformation("Attempting to delete image: {ImageUrl}", imageUrl);

        try
        {
            var fileName = ExtractFileNameFromUrl(imageUrl);
            if (string.IsNullOrEmpty(fileName))
            {
                logger.LogWarning("Could not extract filename from URL: {ImageUrl}", imageUrl);
                return;
            }

            logger.LogInformation("Extracted filename: {FileName}", fileName);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
            logger.LogInformation("Checking file path: {FilePath}", filePath);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                logger.LogInformation("Image file deleted successfully: {FilePath}", filePath);
            }
            else
            {
                logger.LogWarning("Image file not found for deletion: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            // Log error nhưng không throw exception
            // Việc không xóa được file cũ không nên làm fail toàn bộ operation
            logger.LogError(ex, "Failed to delete image: {ImageUrl}", imageUrl);
        }
    }

    private void ValidateImage(IFormFile image)
    {
        logger.LogDebug("Validating image - Size: {Size}, Extension: {Extension}, ContentType: {ContentType}", 
            image.Length, Path.GetExtension(image.FileName), image.ContentType);

        if (image.Length > MaxFileSize)
        {
            logger.LogError("Image size {Size} exceeds maximum allowed size {MaxSize}", 
                image.Length, MaxFileSize);
            throw new InvalidOperationException("File size exceeds 5MB limit.");
        }

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            logger.LogError("Invalid file extension: {Extension}. Allowed: {AllowedExtensions}", 
                extension, string.Join(", ", AllowedExtensions));
            throw new InvalidOperationException("Invalid file type. Only images are allowed.");
        }

        // Bỏ kiểm MIME type (nếu muốn chấp nhận tất cả)
        logger.LogDebug("Image validation completed successfully");
    }


    private string GenerateImageUrl(string fileName)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            logger.LogError("HTTP context is not available for URL generation");
            throw new InvalidOperationException("HTTP context is not available.");
        }

        var url = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/uploads/{fileName}";
        logger.LogDebug("Generated image URL: {Url}", url);
        return url;
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