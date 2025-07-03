using System.ComponentModel.DataAnnotations;
using IngredientServer.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace IngredientServer.Utils.DTOs.Entity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class CreateIngredientRequestDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Quantity { get; set; }
    
    [Required]
    public IngredientUnit Unit { get; set; }
    
    [Required]
    public IngredientCategory Category { get; set; }
    
    [Required]
    public DateTime ExpiryDate { get; set; }

    public IFormFile? Image { get; set; } 
    
    
    public void NormalizeExpiryDate()
    {
        // Nếu thời gian không có timezone, giả định là local và chuyển sang UTC
        ExpiryDate = ExpiryDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(ExpiryDate, DateTimeKind.Local).ToUniversalTime() :
            // Nếu đã có timezone (Local hoặc Utc), chuyển về UTC
            ExpiryDate.ToUniversalTime();
    }

}

public class UpdateIngredientRequestDto : CreateIngredientRequestDto
{
    [Required]
    public int Id { get; set; }
}

public class DeleteIngredientRequestDto
{
    [Required]
    [JsonPropertyName("id")]
    public int Id { get; set; }
}

public class IngredientDataResponseDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public decimal Quantity { get; set; }
    
    public IngredientUnit Unit { get; set; }
    
    public IngredientCategory Category { get; set; }
    
    public DateTime ExpiryDate { get; set; }

    public string? ImageUrl { get; set; } 
    
    
    public void NormalizeExpiryDate()
    {
        // Nếu thời gian không có timezone, giả định là local và chuyển sang UTC
        ExpiryDate = ExpiryDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(ExpiryDate, DateTimeKind.Local).ToUniversalTime() :
            // Nếu đã có timezone (Local hoặc Utc), chuyển về UTC
            ExpiryDate.ToUniversalTime();
    }

}

public class IngredientSearchResultDto
{
    public IEnumerable<IngredientDataResponseDto> Ingredients { get; set; } = new List<IngredientDataResponseDto>();
    public int TotalCount { get; set; }
}
    
// Filtering DTOs
public class IngredientFilterDto
{
    public IngredientCategory? Category { get; set; }
    public bool? IsExpired { get; set; }
    public string? SearchTerm { get; set; }
        
    // Sorting
    public string? SortBy { get; set; } // "name", "quantity", "expiryDate", "category", "createdAt"
    public string? SortDirection { get; set; } // "asc", "desc"
}