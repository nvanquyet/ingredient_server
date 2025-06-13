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

    public class Meal : BaseEntity
    {
        [Required]
        public MealType MealType { get; set; }

        [Required]
        public DateTime MealDate { get; set; }

        [Required]
        public DateTime ConsumedAt { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
        
        public ICollection<MealFood> MealFoods { get; set; } = new List<MealFood>();
        public int FoodCount => MealFoods.Count;
    }
}