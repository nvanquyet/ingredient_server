using System.ComponentModel.DataAnnotations;
using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs.Ingredient
{
    public class CreateIngredientDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Required]
        public IngredientUnit Unit { get; set; }

        [Required]
        public IngredientCategory Category { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class UpdateIngredientDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        public IngredientUnit Unit { get; set; }

        [Required]
        public IngredientCategory Category { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class IngredientResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoon { get; set; }
    }

    public class IngredientFilterDto
    {
        // Loại bỏ UserId - sẽ lấy từ JWT token
        
        public IngredientCategory? Category { get; set; }
        public IngredientUnit? Unit { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsExpiringSoon { get; set; }
        public DateTime? ExpiryDateFrom { get; set; }
        public DateTime? ExpiryDateTo { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class IngredientSortDto
    {
        public string SortBy { get; set; } = "name"; // name, expirydate, quantity, createdat
        public string SortOrder { get; set; } = "asc"; // asc, desc
    }
}