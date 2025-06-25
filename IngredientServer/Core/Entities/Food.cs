using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IngredientServer.Core.Entities
{
    public class Food : BaseEntity
    {
        [Required] [StringLength(200)] public string Name { get; set; } = string.Empty;

        [StringLength(1000)] public string? Description { get; set; }
        
        
        [Required]
        [Range(0, double.MaxValue)]
        public double Quantity { get; set; } 
    
        [Required]
        [Range(0, double.MaxValue)]
        public double Calories { get; set; } 
    
        [Required]
        [Range(0, double.MaxValue)]
        public double Protein { get; set; } 
    
        [Required]
        [Range(0, double.MaxValue)]
        public double Carbs { get; set; } 
    
        [Required]
        [Range(0, double.MaxValue)]
        public double Fat { get; set; } 
        

        public User User { get; set; } = null!;
        public ICollection<FoodIngredient> FoodIngredients { get; set; } = new List<FoodIngredient>();
        public ICollection<MealFood> MealFoods { get; set; } = new List<MealFood>();
    }
}
