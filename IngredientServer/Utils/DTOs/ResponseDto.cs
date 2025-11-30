namespace IngredientServer.Utils.DTOs;

/// <summary>
/// Standard API response wrapper for controllers.
/// Used for all API endpoints to provide consistent response format.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, List<string>?> Metadata { get; set; } = new();
}

/// <summary>
/// Error response DTO for validation errors
/// </summary>
public class ErrorDto
{
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public Dictionary<string, List<string>?>? ValidationErrors { get; set; }
}