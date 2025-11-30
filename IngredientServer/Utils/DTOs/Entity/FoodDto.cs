using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using IngredientServer.Core.Entities;
using IngredientServer.Core.Helpers;
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
        
        // Remaining quantity after deduction (for create/update food response)
        public decimal? RemainingQuantity { get; set; }
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

        public DateTime MealDate { get; set; } = DateTimeHelper.UtcNow;
        
        public MealType MealType { get; set; } = MealType.Breakfast;
        
        public DateTime? ConsumedAt { get; set; } = null;
        
        public IEnumerable<FoodIngredientDto> Ingredients { get; set; } = new List<FoodIngredientDto>();
        
        public void NormalizeConsumedAt()
        {
            ConsumedAt ??= DateTimeHelper.UtcNow;
            ConsumedAt = DateTimeHelper.NormalizeToUtc(ConsumedAt);
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public class UpdateFoodRequestDto : CreateFoodRequestDto
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }


        public override string ToString()
        {
            //return json
            // This method is used to convert the object to JSON string for logging or debugging purposes
            // You can use any JSON serialization library here, e.g., System.Text.Json, Newtonsoft.Json, etc.
            // Assuming you have a method to convert this object to JSON string
            // For example, using System.Text.Json:
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public class DeleteFoodRequestDto
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }
        public override string ToString()
        {
            // This method is used to convert the object to JSON string for logging or debugging purposes
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
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
        public DateTime MealDate { get; set; } = DateTimeHelper.UtcNow;
        
        public DateTime? ConsumedAt { get; set; } = null;
        public IEnumerable<FoodIngredientDto> Ingredients { get; set; } = new List<FoodIngredientDto>();
        
        public void NormalizeConsumedAt()
        {
            ConsumedAt = DateTimeHelper.NormalizeToUtc(ConsumedAt);
        }
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
    
    
    public class FoodAnalysticResponseDto : FoodDataResponseDto
    {
        
    }
    
    public class FoodAnalysticRequestDto
    {
        public IFormFile? Image { get; set; } = null!;
    }
    
    public class FoodRecipeRequestDto
    {
        [Required]
        public string FoodName { get; set; } = string.Empty;
        public IEnumerable<FoodIngredientDto>? Ingredients { get; set; }
    }
}