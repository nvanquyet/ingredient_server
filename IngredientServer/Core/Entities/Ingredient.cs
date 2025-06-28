using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IngredientServer.Core.Entities
{
    public enum IngredientUnit
    {
        Kilogram = 0,
        Liter = 1,
        Piece = 2,
        Box = 3,
        Gram = 4,
        Milliliter = 5,
        Can = 6,
        Cup = 7,
        Tablespoon = 8,
        Teaspoon = 9,
        Package = 10,
        Bottle = 11,
        Other = 12
    }

    public enum IngredientCategory 
    {
        Dairy = 0,
        Meat = 1,
        Vegetables = 2,
        Fruits = 3,
        Grains = 4,
        Seafood = 5,     
        Beverages = 6,   
        Condiments = 7,  
        Snacks = 8,      
        Frozen = 9,      
        Canned = 10,      
        Spices = 11,    
        Other = 12        
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
        
        public string ExpiryDateUtc => DateTime.SpecifyKind(ExpiryDate, DateTimeKind.Utc)
            .ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}