using System.ComponentModel.DataAnnotations;
using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs.Entity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class IngredientDataDto
{
    [Required]
    [StringLength(200)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [Required]
    [Range(0.1, double.MaxValue)]
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
    
    [Required]
    [JsonPropertyName("unit")]
    public IngredientUnit Unit { get; set; }
    
    [Required]
    [JsonPropertyName("category")]
    public IngredientCategory Category { get; set; }
    
    [Required]
    [JsonPropertyName("expiryDate")]
    public DateTime ExpiryDate { get; set; }
    
    [StringLength(500)]
    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}
    
// Response DTOs
public class IngredientDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public IngredientUnit Unit { get; set; }
    public IngredientCategory Category { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? ImageUrl { get; set; }
        
    // Computed properties
    public int DaysUntilExpiry { get; set; }
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
}
    
public class IngredientSearchResultDto
{
    public IEnumerable<IngredientDto> Ingredients { get; set; } = new List<IngredientDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
    
// Filtering DTOs
public class IngredientFilterDto
{
    public IngredientCategory? Category { get; set; }
    public bool? IsExpired { get; set; }
    public bool? IsExpiringSoon { get; set; }
    public string? SearchTerm { get; set; }
        
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
        
    // Sorting
    public string? SortBy { get; set; } // "name", "quantity", "expiryDate", "category", "createdAt"
    public string? SortDirection { get; set; } // "asc", "desc"
}