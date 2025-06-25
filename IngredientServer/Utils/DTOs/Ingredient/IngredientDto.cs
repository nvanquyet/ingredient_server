using System.ComponentModel.DataAnnotations;
using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs.Ingredient;

public class CreateIngredientDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
        
    [StringLength(1000)]
    public string? Description { get; set; }
        
    [Required]
    [Range(0.1, double.MaxValue)]
    public decimal Quantity { get; set; }
        
    [Required]
    public IngredientUnit Unit { get; set; }
        
    [Required]
    public IngredientCategory Category { get; set; }
        
    [Required]
    public DateTime ExpiryDate { get; set; }
        
    [StringLength(500)]
    public string? ImageUrl { get; set; }
}
    
public class UpdateIngredientDto
{
    [StringLength(200)]
    public string? Name { get; set; }
        
    [StringLength(1000)]
    public string? Description { get; set; }
        
    [Range(0.1, double.MaxValue)]
    public decimal? Quantity { get; set; }
        
    public IngredientUnit? Unit { get; set; }
    public IngredientCategory? Category { get; set; }
    public DateTime? ExpiryDate { get; set; }
        
    [StringLength(500)]
    public string? ImageUrl { get; set; }
}
    
public class IngredientDeductionDto
{
    [Required]
    public int IngredientId { get; set; }
        
    [Required]
    [Range(0.1, double.MaxValue)]
    public decimal Quantity { get; set; }
        
    [Required]
    public IngredientUnit Unit { get; set; }
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
        
    // Computed properties
    public int DaysUntilExpiry { get; set; }
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
        
    // Status indicators
    public string Status { get; set; } = string.Empty; // "Available", "Low Stock", "Expired", "Expiring Soon"
    public string UnitDisplay { get; set; } = string.Empty;
    public string CategoryDisplay { get; set; } = string.Empty;
}
    
public class IngredientStatisticsDto
{
    public int TotalIngredients { get; set; }
    public int ExpiredIngredients { get; set; }
    public int ExpiringSoonIngredients { get; set; }
    public int LowStockIngredients { get; set; }
    public int AvailableIngredients { get; set; }
        
    // Category breakdown
    public Dictionary<IngredientCategory, int> CategoryBreakdown { get; set; } = new();
        
    // Unit breakdown
    public Dictionary<IngredientUnit, int> UnitBreakdown { get; set; } = new();
        
    // Value statistics
    public decimal TotalQuantity { get; set; }
    public decimal AverageQuantity { get; set; }
        
    // Expiry statistics
    public int IngredientsExpiringThisWeek { get; set; }
    public int IngredientsExpiringThisMonth { get; set; }
        
    // Most common categories
    public IEnumerable<CategoryStatDto> TopCategories { get; set; } = new List<CategoryStatDto>();
}
    
public class CategoryStatDto
{
    public IngredientCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalQuantity { get; set; }
    public double Percentage { get; set; }
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
    public IngredientUnit? Unit { get; set; }
    public bool? IsExpired { get; set; }
    public bool? IsExpiringSoon { get; set; }
    public bool? IsLowStock { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? ExpiryDateFrom { get; set; }
    public DateTime? ExpiryDateTo { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
        
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
        
    // Sorting
    public string? SortBy { get; set; } // "name", "quantity", "expiryDate", "category", "createdAt"
    public string? SortDirection { get; set; } // "asc", "desc"
}