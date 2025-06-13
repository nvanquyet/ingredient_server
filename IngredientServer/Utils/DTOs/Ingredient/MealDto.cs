using System.ComponentModel.DataAnnotations;

namespace IngredientServer.Utils.DTOs.Ingredient
{
    public class CreateMealDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime MealDate { get; set; }
        public string? Description { get; set; }
        public int? UserId { get; set; } // Thêm UserId vào DTO
    }

    public class UpdateMealDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime MealDate { get; set; }
        public string? Description { get; set; }
    }

    public class AddFoodToMealDto
    {
        [Required]
        public int FoodId { get; set; }
        [Range(0, double.MaxValue)]
        public decimal PortionWeight { get; set; } = 100;
    }

    public class MealResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime MealDate { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}