using Microsoft.AspNetCore.Http;

namespace IngredientServer.Core.Interfaces.Services;

public interface IImageService
{
    /// <summary>
    /// Lưu ảnh mới và trả về URL
    /// </summary>
    /// <param name="image">File ảnh cần lưu</param>
    /// <returns>URL của ảnh đã lưu, null nếu không có ảnh</returns>
    Task<string?> SaveImageAsync(IFormFile? image);

    /// <summary>
    /// Cập nhật ảnh: xóa ảnh cũ và lưu ảnh mới
    /// </summary>
    /// <param name="newImage">Ảnh mới</param>
    /// <param name="existingImageUrl">URL ảnh cũ cần xóa</param>
    /// <returns>URL của ảnh mới, hoặc URL cũ nếu không có ảnh mới</returns>
    Task<string?> UpdateImageAsync(IFormFile? newImage, string? existingImageUrl);

    /// <summary>
    /// Xóa ảnh theo URL
    /// </summary>
    /// <param name="imageUrl">URL của ảnh cần xóa</param>
    Task DeleteImageAsync(string? imageUrl);
}