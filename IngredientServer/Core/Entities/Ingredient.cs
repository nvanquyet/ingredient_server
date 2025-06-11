using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IngredientServer.Core.Entities
{
    public enum IngredientUnit
    {
        Kilogram,
        Liter,
        Piece,
        Box,
        Gram,
        Milliliter
    }

    public enum IngredientCategory
    {
        Dairy,
        Meat,
        Vegetables,
        Fruits,
        Grains,
        Beverages,
        Condiments,
        Snacks,
        Frozen,
        Canned,
        Spices,
        Other
    }

    // Sửa từ abstract thành concrete class
    public sealed class Ingredient : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [Required]
        public IngredientUnit Unit { get; set; } = IngredientUnit.Piece;

        [Required]
        public IngredientCategory Category { get; set; } = IngredientCategory.Other;

        [Required]
        public DateTime ExpiryDate { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        // Computed properties
        public int DaysUntilExpiry => (ExpiryDate.Date - DateTime.Now.Date).Days;
        public bool IsExpired => DateTime.Now.Date > ExpiryDate.Date;
        public bool IsExpiringSoon => DaysUntilExpiry is <= 7 and >= 0;

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<FoodIngredient> FoodIngredients { get; set; } = new List<FoodIngredient>();
    }
}