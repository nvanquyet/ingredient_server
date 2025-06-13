namespace IngredientServer.Core.Entities
{
     public class FoodSuggestion
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FoodCategory Category { get; set; }
        public CookingMethod CookingMethod { get; set; }
        public int PreparationTimeMinutes { get; set; }
        public List<string> RequiredIngredients { get; set; } = new();
        public List<string> OptionalIngredients { get; set; } = new();
        public string ShortRecipe { get; set; } = string.Empty;
        public NutritionInfo EstimatedNutrition { get; set; } = new();
        public decimal MatchScore { get; set; } // 0-100, độ phù hợp với nguyên liệu hiện có
        public string WhyRecommended { get; set; } = string.Empty;
    }

    public class DetailedRecipe
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<RecipeIngredient> Ingredients { get; set; } = new();
        public List<CookingStep> Steps { get; set; } = new();
        public int PreparationTimeMinutes { get; set; }
        public int CookingTimeMinutes { get; set; }
        public int Servings { get; set; }
        public string Difficulty { get; set; } = string.Empty; // Easy, Medium, Hard
        public NutritionInfo NutritionPerServing { get; set; } = new();
        public List<string> Tips { get; set; } = new();
    }

    public class RecipeIngredient
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsOptional { get; set; }
        public string PreparationNote { get; set; } = string.Empty; // "diced", "sliced", etc.
    }

    public class CookingStep
    {
        public int StepNumber { get; set; }
        public string Instruction { get; set; } = string.Empty;
        public int TimeMinutes { get; set; }
        public string Temperature { get; set; } = string.Empty;
        public List<string> Tips { get; set; } = new();
    }

    public class NutritionInfo
    {
        public decimal Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbs { get; set; }
        public decimal Fats { get; set; }
        public decimal Fiber { get; set; }
        public decimal Sugar { get; set; }
        public decimal Sodium { get; set; }
    }

    public class NutritionAnalysis
    {
        public NutritionInfo Nutrition { get; set; } = new();
        public string HealthScore { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
        public List<string> HealthBenefits { get; set; } = new();
        public List<string> NutritionalConcerns { get; set; } = new();
        public List<string> ImprovementSuggestions { get; set; } = new();
        public bool IsAlignedWithGoal { get; set; }
        public string GoalAlignment { get; set; } = string.Empty;
    }

    public class IngredientSubstitution
    {
        public string OriginalIngredient { get; set; } = string.Empty;
        public string SubstituteIngredient { get; set; } = string.Empty;
        public string ConversionRatio { get; set; } = string.Empty; // "1:1", "2:1", etc.
        public string Reason { get; set; } = string.Empty;
        public string TasteImpact { get; set; } = string.Empty;
        public string NutritionImpact { get; set; } = string.Empty;
    }
}