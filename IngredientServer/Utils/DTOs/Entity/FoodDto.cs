using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IngredientServer.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace IngredientServer.Utils.DTOs.Entity
{
    public class FoodIngredientDto
    {
        [Required]
        public int IngredientId { get; set; }
        
        [Required]
        public decimal Quantity { get; set; }
        
        [Required]
        public IngredientUnit Unit { get; set; }
        
        // For response
        public string? IngredientName { get; set; }
    }
    
    public class CreateFoodRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }
        
        public IFormFile? Image { get; set; }

        [Required]
        public int PreparationTimeMinutes { get; set; }

        [Required]
        public int CookingTimeMinutes { get; set; }

        [Required]
        public decimal Calories { get; set; }

        [Required]
        public decimal Protein { get; set; }

        [Required]
        public decimal Carbohydrates { get; set; }

        [Required]
        public decimal Fat { get; set; }

        [Required]
        public decimal Fiber { get; set; }

        [Required]
        public List<string> Instructions { get; set; } = new List<string>();
        
        [Required]
        public List<string> Tips { get; set; } = new List<string>();

        // Difficulty level (1-5 scale)
        [Range(1, 5, ErrorMessage = "Difficulty must be between 1 and 5")]
        public int DifficultyLevel { get; set; } = 1;

        public DateTime MealDate { get; set; } = DateTime.UtcNow;
        
        public MealType MealType { get; set; } = MealType.Breakfast;
        
        public IEnumerable<FoodIngredientDto> Ingredients { get; set; } = new List<FoodIngredientDto>();
    }

    public class UpdateFoodRequestDto : CreateFoodRequestDto
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class DeleteFoodRequestDto
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class FoodDataResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int PreparationTimeMinutes { get; set; }
        public int CookingTimeMinutes { get; set; }
        public decimal Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbohydrates { get; set; }
        public decimal Fat { get; set; }
        public decimal Fiber { get; set; }
        public List<string> Instructions { get; set; } = new List<string>();
        public List<string> Tips { get; set; } = new List<string>();
        public int DifficultyLevel { get; set; } = 1;
        public MealType MealType { get; set; }
        public DateTime MealDate { get; set; } = DateTime.UtcNow;
        public IEnumerable<FoodIngredientDto> Ingredients { get; set; } = new List<FoodIngredientDto>();
    }
    
    public class FoodSuggestionRequestDto
    {
        public int MaxSuggestions { get; set; } = 5;
        public UserInformationDto UserInformation { get; set; } = new UserInformationDto();
    }
    
    public class FoodSuggestionResponseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public decimal Kcal { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public List<FoodIngredientDto> Ingredients { get; set; } = [];
    }
    
    public class FoodRecipeRequestDto
    {
        [Required]
        public string FoodName { get; set; } = string.Empty;
        public IEnumerable<FoodIngredientDto>? Ingredients { get; set; }
    }
}