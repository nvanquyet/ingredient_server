using System.ComponentModel.DataAnnotations;

namespace IngredientServer.Core.Entities
{
    public enum MealType
    {
        Breakfast,
        Lunch,
        Dinner,
        Snack,
        Other
    }

    public abstract class Meal : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public MealType MealType { get; set; }

        [Required]
        public DateTime MealDate { get; set; }

        [Required]
        public DateTime ConsumedAt { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
        
        public ICollection<MealFood> MealFoods { get; set; } = new List<MealFood>();

        // Computed properties - Tính tổng dinh dưỡng của cả bữa ăn
        public decimal TotalCalories => MealFoods.Sum(mf => 
            (mf.Food.CaloriesPer100G ?? 0) * mf.PortionWeight / 100);

        public decimal TotalProtein => MealFoods.Sum(mf => 
            (mf.Food.ProteinPer100G ?? 0) * mf.PortionWeight / 100);

        public decimal TotalCarbs => MealFoods.Sum(mf => 
            (mf.Food.CarbsPer100G ?? 0) * mf.PortionWeight / 100);

        public decimal TotalFats => MealFoods.Sum(mf => 
            (mf.Food.FatsPer100G ?? 0) * mf.PortionWeight / 100);

        public decimal TotalFiber => MealFoods.Sum(mf => 
            (mf.Food.FiberPer100G ?? 0) * mf.PortionWeight / 100);

        public decimal TotalSodium => MealFoods.Sum(mf => 
            (mf.Food.SodiumPer100G ?? 0) * mf.PortionWeight / 100);

        public int FoodCount => MealFoods.Count;
    }
}