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

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Preparation time must be non-negative")]
        public int PreparationTimeMinutes { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Cooking time must be non-negative")]
        public int CookingTimeMinutes { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Calories must be non-negative")]
        public decimal Calories { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Protein must be non-negative")]
        public decimal Protein { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Carbohydrates must be non-negative")]
        public decimal Carbohydrates { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Fat must be non-negative")]
        public decimal Fat { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Fiber must be non-negative")]
        public decimal Fiber { get; set; }

        // Recipe instructions/steps - stored as JSON in database
        [Required]
        public List<string> Instructions { get; set; } = new List<string>();

        // Difficulty level (1-5 scale)
        [Range(1, 5, ErrorMessage = "Difficulty must be between 1 and 5")]
        public int DifficultyLevel { get; set; } = 1;
         
        [Required]
        public MealType MealType { get; set; }
        
        [Required]
        public DateTime MealDate { get; set; }
        
        public IEnumerable<FoodIngredientDto> Ingredients { get; set; } = new List<FoodIngredientDto>();


        public Food ToFood()
        {
            var food = new Food
            {
                Name = this.Name,
                Description = this.Description,
                PreparationTimeMinutes = this.PreparationTimeMinutes,
                CookingTimeMinutes = this.CookingTimeMinutes,
                Calories = this.Calories,
                Protein = this.Protein,
                Carbohydrates = this.Carbohydrates,
                Fat = this.Fat,
                Fiber = this.Fiber,
                Instructions = this.Instructions,
                DifficultyLevel = this.DifficultyLevel
            };
            return food;
        }

        public static FoodDataDto FromFood(Food food)
        {
            return new FoodDataDto()
            {
                Name = food.Name,
                Description = food.Description,
                PreparationTimeMinutes = food.PreparationTimeMinutes,
                CookingTimeMinutes = food.CookingTimeMinutes,
                Calories = food.Calories,
                Protein = food.Protein,
                Carbohydrates = food.Carbohydrates,
                Fat = food.Fat,
                Fiber = food.Fiber,
                Instructions = food.Instructions ?? new List<string>(),
                DifficultyLevel = food.DifficultyLevel,
                Ingredients = food.FoodIngredients.Select(i => new FoodIngredientDto
                {
                    IngredientId = i.IngredientId,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    IngredientName = i.Ingredient?.Name
                }).ToList()
            };
        }
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
    
  
    
    public class FoodSuggestionRequestDto
    {
        public int MaxSuggestions { get; set; } = 5;
        public UserInformationDto UserInformation { get; set; }
        public IEnumerable<FoodIngredientDto> Ingredients { get ; set; } = new List<FoodIngredientDto>();
    }
    
    public class FoodSuggestionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public decimal Kcal { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public List<FoodIngredientDto> Ingredients { get; set; } = new();
    }
    
    public class FoodRecipeRequestDto
    {
        [Required]
        public string FoodName { get; set; } = string.Empty;
        public IEnumerable<FoodIngredientDto>? Ingredients { get; set; }
    }
}