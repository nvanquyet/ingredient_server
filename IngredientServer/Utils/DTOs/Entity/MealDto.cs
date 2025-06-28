using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs.Entity
{
    public class MealDto
    {
        public int Id { get; set; }
        public MealType MealType { get; set; }
        public DateTime MealDate { get; set; }
        public IEnumerable<MealFoodDto> Foods { get; set; } = new List<MealFoodDto>();
    }
    
    // Response DTOs
    public class MealFoodDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
    }
    
   
}
