using IngredientServer.Core.Configuration;
using IngredientServer.Core.Helpers;
using IngredientServer.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IngredientServer.Core.Services;

public class ImageService : IImageService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly FileUploadOptions _options;
    private readonly string[] _allowedExtensions;
    private readonly ILogger<ImageService> _logger;
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };

    public ImageService(
        IHttpContextAccessor httpContextAccessor,
        IOptions<FileUploadOptions> fileUploadOptions,
        ILogger<ImageService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = fileUploadOptions.Value;
        _logger = logger;
        _allowedExtensions = _options.GetAllowedExtensionsArray();
    }

    public async Task<string?> SaveImageAsync(IFormFile? image)
    {
        _logger.LogInformation("=== START IMAGE SAVE OPERATION ===");
        
        if (image == null || image.Length == 0)
        {
            _logger.LogInformation("No image provided or empty image");
            return null;
        }

        _logger.LogInformation("Processing image - FileName: {FileName}, Size: {Size} bytes, ContentType: {ContentType}", 
            image.FileName, image.Length, image.ContentType);

        try
        {
            ValidateImage(image);
            _logger.LogInformation("Image validation passed");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            
            if (!Directory.Exists(uploadsFolder))
            {
                _logger.LogInformation("Creating uploads directory: {UploadsFolder}", uploadsFolder);
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            _logger.LogInformation("Saving image to: {FilePath}", filePath);

            var startTime = DateTimeHelper.UtcNow;
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(fileStream);
            var saveTime = DateTimeHelper.UtcNow.Subtract(startTime).TotalMilliseconds;

            _logger.LogInformation("Image saved successfully in {SaveTime}ms", saveTime);

            var imageUrl = GenerateImageUrl(uniqueFileName);
            _logger.LogInformation("Generated image URL: {ImageUrl}", imageUrl);
            
            return imageUrl;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Image validation failed for file: {FileName}", image.FileName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image: {FileName}", image.FileName);
            throw new InvalidOperationException("Failed to save image.", ex);
        }
    }

    public async Task<string?> UpdateImageAsync(IFormFile? newImage, string? existingImageUrl)
    {
        _logger.LogInformation("=== START IMAGE UPDATE OPERATION ===");
        _logger.LogInformation("Existing image URL: {ExistingUrl}", existingImageUrl ?? "None");
        
        // Nếu không có ảnh mới, giữ nguyên ảnh cũ
        if (newImage == null || newImage.Length == 0)
        {
            _logger.LogInformation("No new image provided, keeping existing image");
            return existingImageUrl;
        }

        _logger.LogInformation("New image provided - FileName: {FileName}, Size: {Size} bytes", 
            newImage.FileName, newImage.Length);

        try
        {
            // Xóa ảnh cũ trước (nếu có)
            if (!string.IsNullOrEmpty(existingImageUrl))
            {
                _logger.LogInformation("Deleting old image before saving new one");
                await DeleteImageAsync(existingImageUrl);
            }

            // Lưu ảnh mới
            var newImageUrl = await SaveImageAsync(newImage);
            _logger.LogInformation("Image update completed successfully. New URL: {NewUrl}", newImageUrl);
            
            return newImageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update image from {OldUrl} to new image {NewFileName}", 
                existingImageUrl, newImage.FileName);
            throw new InvalidOperationException("Failed to update image.", ex);
        }
    }

    public async Task DeleteImageAsync(string? imageUrl)
    {
        _logger.LogInformation("=== START IMAGE DELETE OPERATION ===");
        
        if (string.IsNullOrEmpty(imageUrl))
        {
            _logger.LogInformation("No image URL provided for deletion");
            return;
        }

        _logger.LogInformation("Attempting to delete image: {ImageUrl}", imageUrl);

        try
        {
            var fileName = ExtractFileNameFromUrl(imageUrl);
            if (string.IsNullOrEmpty(fileName))
            {
                _logger.LogWarning("Could not extract filename from URL: {ImageUrl}", imageUrl);
                return;
            }

            _logger.LogInformation("Extracted filename: {FileName}", fileName);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
            _logger.LogInformation("Checking file path: {FilePath}", filePath);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                _logger.LogInformation("Image file deleted successfully: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("Image file not found for deletion: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            // Log error nhưng không throw exception
            // Việc không xóa được file cũ không nên làm fail toàn bộ operation
            _logger.LogError(ex, "Failed to delete image: {ImageUrl}", imageUrl);
        }
    }

    private void ValidateImage(IFormFile image)
    {
        _logger.LogDebug("Validating image - Size: {Size}, Extension: {Extension}, ContentType: {ContentType}", 
            image.Length, Path.GetExtension(image.FileName), image.ContentType);

        if (image.Length > _options.MaxFileSize)
        {
            _logger.LogError("Image size {Size} exceeds maximum allowed size {MaxSize}", 
                image.Length, _options.MaxFileSize);
            throw new InvalidOperationException($"File size exceeds {_options.MaxFileSize / (1024 * 1024)}MB limit.");
        }

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            _logger.LogError("Invalid file extension: {Extension}. Allowed: {AllowedExtensions}", 
                extension, string.Join(", ", _allowedExtensions));
            throw new InvalidOperationException("Invalid file type. Only images are allowed.");
        }

        // Bỏ kiểm MIME type (nếu muốn chấp nhận tất cả)
        _logger.LogDebug("Image validation completed successfully");
    }


    private string GenerateImageUrl(string fileName)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogError("HTTP context is not available for URL generation");
            throw new InvalidOperationException("HTTP context is not available.");
        }

        var url = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/uploads/{fileName}";
        _logger.LogDebug("Generated image URL: {Url}", url);
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