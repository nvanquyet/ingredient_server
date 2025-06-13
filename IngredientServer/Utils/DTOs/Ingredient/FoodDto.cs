using System.ComponentModel.DataAnnotations;
using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs.Ingredient
{
    public class CreateFoodDto
    {
        [Required] public string Name { get; set; }
        public FoodCategory Category { get; set; }
        public string? Recipe { get; set; }
        public int? PreparationTimeMinutes { get; set; }
        public int? UserId { get; set; } // Thêm UserId vào DTO
    }

    public class UpdateFoodDto
    {
        [Required] public string Name { get; set; }
        public FoodCategory Category { get; set; }
        public string? Recipe { get; set; }
        public int? PreparationTimeMinutes { get; set; }
    }

    public class FoodResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string? Recipe { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}