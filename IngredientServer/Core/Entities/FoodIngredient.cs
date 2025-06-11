using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IngredientServer.Core.Entities
{
    // Entity trung gian để liên kết Food và Ingredient với số lượng cụ thể
    public abstract class FoodIngredient : BaseEntity
    {
        [Required]
        public int FoodId { get; set; }

        [Required]
        public int IngredientId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [Required]
        public IngredientUnit Unit { get; set; }

        // Navigation properties
        public Food Food { get; set; } = null!;
        public Ingredient Ingredient { get; set; } = null!;
    }
}