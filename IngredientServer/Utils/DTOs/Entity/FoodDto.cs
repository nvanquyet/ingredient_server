using System.ComponentModel.DataAnnotations;
using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs.Entity
{
     // Request DTOs
    public class FoodDataDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        [Range(0.1, double.MaxValue)]
        public double Quantity { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public double Calories { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public double Protein { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public double Carbs { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public double Fat { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public MealType MealType { get; set; }
        
        public IEnumerable<FoodIngredientDto> Ingredients { get; set; } = new List<FoodIngredientDto>();
    }
    
    public class FoodIngredientDto
    {
        [Required]
        public int IngredientId { get; set; }
        
        [Required]
        [Range(0.1, double.MaxValue)]
        public decimal Quantity { get; set; }
        
        [Required]
        public IngredientUnit Unit { get; set; }
        
        // For response
        public string? IngredientName { get; set; }
    }
    
    // Response DTOs
    public class FoodDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Quantity { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IEnumerable<FoodIngredientDto> Ingredients { get; set; } = new List<FoodIngredientDto>();
    }
    
    public class FoodSuggestionRequestDto
    {
        public List<int>? IngredientIds { get; set; }
        public NutritionGoal NutritionGoal { get; set; } = NutritionGoal.Balanced;
        public int MaxSuggestions { get; set; } = 5;
        public UserInformationDto UserInformation { get; set; }
    }
    
    public class FoodSuggestionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
    }
    
    public class FoodRecipeDto
    {
        public string FoodName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PrepTime { get; set; } = string.Empty;
        public string CookTime { get; set; } = string.Empty;
        public string TotalTime { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public double EstimatedCalories { get; set; }
        public double EstimatedProtein { get; set; }
        public double EstimatedCarbs { get; set; }
        public double EstimatedFat { get; set; }
        public IEnumerable<RecipeIngredientDto> Ingredients { get; set; } = new List<RecipeIngredientDto>();
        public IEnumerable<string> Instructions { get; set; } = new List<string>();
        public string? Tips { get; set; }
    }
    
    public class RecipeIngredientDto
    {
        public string Name { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
    }
    
    public class FoodRecipeRequestDto
    {
        [Required]
        public string FoodName { get; set; } = string.Empty;
        public IEnumerable<string>? Ingredients { get; set; }
    }
}