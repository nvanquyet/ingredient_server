namespace IngredientServer.Core.Entities;

public class UserNutritionTargets : BaseEntity
{
    public decimal TargetDailyCalories { get; set; }
    public decimal TargetDailyProtein { get; set; }
    public decimal TargetDailyCarbohydrates { get; set; }
    public decimal TargetDailyFat { get; set; }
    public decimal TargetDailyFiber { get; set; }
}