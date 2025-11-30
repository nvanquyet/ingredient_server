namespace IngredientServer.Utils.DTOs.Common;

/// <summary>
/// Internal service response DTO.
/// Used for service layer responses (e.g., AuthService).
/// For API responses, use ApiResponse&lt;T&gt; instead.
/// </summary>
public class ResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}