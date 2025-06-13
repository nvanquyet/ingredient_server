
// Request/Response DTOs

using IngredientServer.Core.Entities;

namespace IngredientServer.Core.DTOs
{
    public class FoodSuggestionRequest
    {
        public List<int>? IngredientIds { get; set; }
        public NutritionGoal NutritionGoal { get; set; } = NutritionGoal.Balanced;
        public int MaxSuggestions { get; set; } = 5;
    }

    public class GenerateRecipeRequest
    {
        public string FoodName { get; set; } = string.Empty;
        public List<int> IngredientIds { get; set; } = new();
        public NutritionGoal NutritionGoal { get; set; } = NutritionGoal.Balanced;
    }

    public class IngredientSubstitutionRequest
    {
        public string OriginalIngredient { get; set; } = string.Empty;
        public NutritionGoal NutritionGoal { get; set; } = NutritionGoal.Balanced;
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class WeeklyMealPlanRequest
    {
        public int DaysCount { get; set; } = 7;
        public int MealsPerDay { get; set; } = 3;
        public NutritionGoal NutritionGoal { get; set; } = NutritionGoal.Balanced;
        public decimal Budget { get; set; }
        public List<string> DietaryRestrictions { get; set; } = new();
    }

    public class FreshnessAssessmentRequest
    {
        public string IngredientName { get; set; } = string.Empty;
        public string ImageBase64 { get; set; } = string.Empty;
    }

    // Response DTOs
    public class WeeklyMealPlan
    {
        public DateTime StartDate { get; set; }
        public int DaysCount { get; set; }
        public List<DailyMealPlan> DailyMealPlans { get; set; } = new();
        public decimal TotalEstimatedCost { get; set; }
    }

    public class DailyMealPlan
    {
        public DateTime Date { get; set; }
        public List<MealPlan> Meals { get; set; } = new();
        public int TotalCalories { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class MealPlan
    {
        public string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, Snack
        public string FoodName { get; set; } = string.Empty;
        public List<string> Ingredients { get; set; } = new();
        public int EstimatedCalories { get; set; }
        public int PreparationTime { get; set; }
        public string? RecipeUrl { get; set; }
    }

    public class IngredientFreshnessAssessment
    {
        public string IngredientName { get; set; } = string.Empty;
        public int FreshnessScore { get; set; } // 0-100
        public string FreshnessLevel { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
        public int EstimatedShelfLife { get; set; } // days
        public List<string> StorageTips { get; set; } = new();
        public List<string> QualityIndicators { get; set; } = new();
        public List<string> WarningSignals { get; set; } = new();
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object>? Metadata { get; set; }
    }
}