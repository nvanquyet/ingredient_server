using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs.Entity
{
    // Response DTOs
    public class MealDto
    {
        public int Id { get; set; }
        public MealType MealType { get; set; }
        public DateTime MealDate { get; set; }
        public DateTime? ConsumedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    public class MealWithFoodsDto
    {
        public int Id { get; set; }
        public MealType MealType { get; set; }
        public DateTime MealDate { get; set; }
        public DateTime? ConsumedAt { get; set; }
        public int FoodCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IEnumerable<FoodDto> Foods { get; set; } = new List<FoodDto>();
    }
    
    public class MealNutritionSummaryDto
    {
        public int MealId { get; set; }
        public MealType MealType { get; set; }
        public DateTime MealDate { get; set; }
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
        public int FoodCount { get; set; }
        
        // Percentage breakdown
        public double ProteinPercentage { get; set; }
        public double CarbsPercentage { get; set; }
        public double FatPercentage { get; set; }
    }
    
    public class DailyNutritionSummaryDto
    {
        public DateTime Date { get; set; }
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
        public int TotalMeals { get; set; }
        public int TotalFoods { get; set; }
        
        // User's targets (if available)
        public double? TargetCalories { get; set; }
        public double? TargetProtein { get; set; }
        public double? TargetCarbs { get; set; }
        public double? TargetFat { get; set; }
        
        // Progress percentages
        public double? CaloriesProgress { get; set; }
        public double? ProteinProgress { get; set; }
        public double? CarbsProgress { get; set; }
        public double? FatProgress { get; set; }
        
        public IEnumerable<MealNutritionSummaryDto> MealBreakdown { get; set; } = new List<MealNutritionSummaryDto>();
    }
    
    public class WeeklyNutritionSummaryDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public double AverageCalories { get; set; }
        public double AverageProtein { get; set; }
        public double AverageCarbs { get; set; }
        public double AverageFat { get; set; }
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
        public int TotalMeals { get; set; }
        public int TotalFoods { get; set; }
        
        public IEnumerable<DailyNutritionSummaryDto> DailyBreakdown { get; set; } = new List<DailyNutritionSummaryDto>();
    }

    public class TotalNutritionSummaryDto
    {
        public double AverageCalories { get; set; }
        public double AverageProtein { get; set; }
        public double AverageCarbs { get; set; }
        public double AverageFat { get; set; }
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
        public int TotalMeals { get; set; }
        public int TotalFoods { get; set; }
    }
    
}
