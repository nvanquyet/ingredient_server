using IngredientServer.Core.Helpers;

namespace IngredientServer.Utils.DTOs.Common;

/// <summary>
/// Error response DTO for middleware error handling
/// </summary>
public class ErrorResponseDto
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTimeHelper.UtcNow;
    public string Path { get; set; } = string.Empty;
}

