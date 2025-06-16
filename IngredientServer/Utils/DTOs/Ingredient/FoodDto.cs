using System.ComponentModel.DataAnnotations;
using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs.Ingredient
{
    public class CreateFoodDto
    {
        [Required] 
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public FoodCategory Category { get; set; }
        
        [StringLength(2000)]
        public string? Recipe { get; set; }
        
        [Range(1, 1440, ErrorMessage = "Preparation time must be between 1 and 1440 minutes")]
        public int? PreparationTimeMinutes { get; set; }
        
        // Loại bỏ UserId - sẽ lấy từ JWT token
    }

    public class UpdateFoodDto
    {
        [Required] 
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public FoodCategory Category { get; set; }
        
        [StringLength(2000)]
        public string? Recipe { get; set; }
        
        [Range(1, 1440, ErrorMessage = "Preparation time must be between 1 and 1440 minutes")]
        public int? PreparationTimeMinutes { get; set; }
    }

    public class FoodResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Recipe { get; set; }
        public int? PreparationTimeMinutes { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FoodListResponseDto
    {
        public List<FoodResponseDto> Foods { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}