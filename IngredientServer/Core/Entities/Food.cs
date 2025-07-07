using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Entities
{
    public class Food : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int PreparationTimeMinutes { get; set; }

        [Required]
        public int CookingTimeMinutes { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Calories { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Protein { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Carbohydrates { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Fat { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Fiber { get; set; }
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        public DateTime? ConsumedAt { get; set; } = null;


        // Recipe instructions/steps - stored as JSON in database
        [Required]
        [Column(TypeName = "json")]
        public List<string> Instructions { get; set; } = new List<string>();
        
        [Required]
        [Column(TypeName = "json")]
        public List<string> Tips { get; set; } = new List<string>();

        // Difficulty level (1-5 scale)
        [Range(1, 5, ErrorMessage = "Difficulty must be between 1 and 5")]
        public int DifficultyLevel { get; set; } = 1;

        public User User { get; set; } = null!;
        
        public ICollection<FoodIngredient> FoodIngredients { get; set; } = new List<FoodIngredient>();
        public ICollection<MealFood> MealFoods { get; set; } = new List<MealFood>();

        // Computed property for total time
        [NotMapped]
        public int TotalTimeMinutes => PreparationTimeMinutes + CookingTimeMinutes;
        
    }
}