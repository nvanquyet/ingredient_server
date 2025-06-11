using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IngredientServer.Core.Entities
{
    public enum FoodCategory
    {
        MainDish,
        SideDish,
        Soup,
        Salad,
        Dessert,
        Appetizer,
        Beverage,
        Snack,
        Sauce,
        Other
    }

    public enum CookingMethod
    {
        Raw,
        Boiled,
        Fried,
        Grilled,
        Baked,
        Steamed,
        Roasted,
        Stewed,
        Other
    }
    
    public sealed class Food : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public FoodCategory Category { get; set; } = FoodCategory.Other;

        public CookingMethod CookingMethod { get; set; } = CookingMethod.Other;

        [StringLength(2000)]
        public string? Recipe { get; set; }

        [Range(1, 1440)]
        public int? PreparationTimeMinutes { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? CaloriesPer100G { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ProteinPer100G { get; set; } // gram

        [Column(TypeName = "decimal(8,2)")]
        public decimal? CarbsPer100G { get; set; } // gram

        [Column(TypeName = "decimal(8,2)")]
        public decimal? FatsPer100G { get; set; } // gram

        [Column(TypeName = "decimal(8,2)")]
        public decimal? FiberPer100G { get; set; } // gram

        [Column(TypeName = "decimal(8,2)")]
        public decimal? SugarPer100G { get; set; } // gram

        [Column(TypeName = "decimal(8,2)")]
        public decimal? SodiumPer100G { get; set; } // mg

        // Trọng lượng chuẩn cho 1 phần ăn (gram)
        [Column(TypeName = "decimal(8,2)")]
        public decimal StandardPortionWeight { get; set; } = 100;

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<FoodIngredient> FoodIngredients { get; set; } = new List<FoodIngredient>();
        public ICollection<MealFood> MealFoods { get; set; } = new List<MealFood>();

        // Computed properties
        public decimal CaloriesPerPortion => CaloriesPer100G.HasValue 
            ? (CaloriesPer100G.Value * StandardPortionWeight) / 100 
            : 0;

        public decimal ProteinPerPortion => ProteinPer100G.HasValue 
            ? (ProteinPer100G.Value * StandardPortionWeight) / 100 
            : 0;

        public decimal CarbsPerPortion => CarbsPer100G.HasValue 
            ? (CarbsPer100G.Value * StandardPortionWeight) / 100 
            : 0;

        public decimal FatsPerPortion => FatsPer100G.HasValue 
            ? (FatsPer100G.Value * StandardPortionWeight) / 100 
            : 0;
    }
}

