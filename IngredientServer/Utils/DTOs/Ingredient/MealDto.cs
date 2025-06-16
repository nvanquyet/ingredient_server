using System.ComponentModel.DataAnnotations;

namespace IngredientServer.Utils.DTOs.Ingredient
{
    public class CreateMealDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public DateTime MealDate { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // Loại bỏ UserId - sẽ lấy từ JWT token
    }

    public class UpdateMealDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public DateTime MealDate { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class AddFoodToMealDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "FoodId must be greater than 0")]
        public int FoodId { get; set; }
        
        [Required]
        [Range(0.1, 10000, ErrorMessage = "Portion weight must be between 0.1 and 10000")]
        public decimal PortionWeight { get; set; } = 100;
    }

    public class MealResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime MealDate { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class MealDetailsResponseDto : MealResponseDto
    {
        public List<MealFoodDto> Foods { get; set; } = new();
        public int TotalCalories { get; set; }
        public decimal TotalWeight { get; set; }
    }

    public class MealFoodDto
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public decimal PortionWeight { get; set; }
        public int EstimatedCalories { get; set; }
    }

    public class MealListResponseDto
    {
        public List<MealResponseDto> Meals { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}