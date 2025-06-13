using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IngredientServer.Core.Entities
{
    // Entity trung gian để liên kết Meal và Food với khối lượng cụ thể
    public class MealFood : BaseEntity
    {
        [Required]
        public int MealId { get; set; }

        [Required]
        public int FoodId { get; set; }

        // Navigation properties
        public Meal Meal { get; set; } = null!;
        public Food Food { get; set; } = null!;
    }
}