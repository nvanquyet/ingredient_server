using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IngredientServer.Core.Entities
{
    public enum NutritionGoal
    {
        Balanced,           // Cân bằng dinh dưỡng
        WeightLoss,         // Giảm cân
        WeightGain,         // Tăng cân
        MuscleGain,         // Tăng cơ
        LowCarb,            // Ít carb
        HighProtein,        // Nhiều protein
        Vegetarian,         // Ăn chay
        Vegan,              // Ăn thuần chay
        Keto,               // Chế độ Keto
        Mediterranean,      // Địa Trung Hải
        Paleo,              // Paleo
        LowSodium,          // Ít muối
        DiabeticFriendly,   // Phù hợp tiểu đường
        HeartHealthy,       // Bảo vệ tim mạch
        AntiInflammatory,   // Chống viêm
        Other               // Khác
    }

    public enum ActivityLevel
    {
        Sedentary,      // Ít vận động
        Light,          // Vận động nhẹ
        Moderate,       // Vận động vừa
        Active,         // Vận động nhiều
        VeryActive      // Vận động rất nhiều
    }

    public enum Gender
    {
        Male,
        Female,
        Other,
        PreferNotToSay
    }

    public class User : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        // Thông tin cá nhân cho tính toán dinh dưỡng
        public DateTime? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Height { get; set; } // cm

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; } // kg

        [Column(TypeName = "decimal(5,2)")]
        public decimal? TargetWeight { get; set; } // kg - Cân nặng mục tiêu

        // Mục tiêu dinh dưỡng và lối sống
        public NutritionGoal PrimaryNutritionGoal { get; set; } = NutritionGoal.Balanced;
        
        public NutritionGoal? SecondaryNutritionGoal { get; set; } // Mục tiêu phụ

        public ActivityLevel ActivityLevel { get; set; } = ActivityLevel.Sedentary;

        // Mục tiêu hàng ngày (có thể tự động tính hoặc người dùng tự đặt)
        [Column(TypeName = "decimal(7,2)")]
        public decimal? DailyCalorieGoal { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? DailyProteinGoal { get; set; } // gram

        [Column(TypeName = "decimal(6,2)")]
        public decimal? DailyCarbGoal { get; set; } // gram

        [Column(TypeName = "decimal(6,2)")]
        public decimal? DailyFatGoal { get; set; } // gram

        [Column(TypeName = "decimal(6,2)")]
        public decimal? DailyFiberGoal { get; set; } // gram

        [Column(TypeName = "decimal(8,2)")]
        public decimal? DailySodiumLimit { get; set; } // mg

        // Ràng buộc ăn uống
        public bool HasFoodAllergies { get; set; } = false;
        
        [StringLength(1000)]
        public string? FoodAllergies { get; set; } 
        
        [StringLength(1000)]
        public string? FoodPreferences { get; set; } 

        [StringLength(1000)]
        public string? FoodRestrictions { get; set; } 

        // Cài đặt ứng dụng
        public bool EnableNotifications { get; set; } = true;
        
        public bool EnableMealReminders { get; set; } = true;
        
        public TimeSpan? BreakfastReminderTime { get; set; }
        
        public TimeSpan? LunchReminderTime { get; set; }
        
        public TimeSpan? DinnerReminderTime { get; set; }

        // Navigation properties
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        public ICollection<Food> Foods { get; set; } = new List<Food>();
        public ICollection<Meal> Meals { get; set; } = new List<Meal>();

        // Computed properties
        public int? Age => DateOfBirth.HasValue 
            ? DateTime.Now.Year - DateOfBirth.Value.Year - (DateTime.Now.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)
            : null;

        public decimal? BMI => Height.HasValue && Weight.HasValue && Height > 0 
            ? Math.Round(Weight.Value / (decimal)Math.Pow((double)(Height.Value / 100), 2), 2)
            : null;


        public string FullName => $"{FirstName} {LastName}".Trim();

        // BMR (Basal Metabolic Rate) calculation using Mifflin-St Jeor Equation
        public decimal? BMR
        {
            get
            {
                if (!Weight.HasValue || !Height.HasValue || !Age.HasValue || !Gender.HasValue)
                    return null;

                decimal bmr = Gender == Entities.Gender.Male
                    ? (10 * Weight.Value) + (6.25m * Height.Value) - (5 * Age.Value) + 5
                    : (10 * Weight.Value) + (6.25m * Height.Value) - (5 * Age.Value) - 161;

                return Math.Round(bmr, 2);
            }
        }

        // TDEE (Total Daily Energy Expenditure)
        public decimal? TDEE
        {
            get
            {
                if (!BMR.HasValue) return null;

                decimal multiplier = ActivityLevel switch
                {
                    ActivityLevel.Sedentary => 1.2m,
                    ActivityLevel.Light => 1.375m,
                    ActivityLevel.Moderate => 1.55m,
                    ActivityLevel.Active => 1.725m,
                    ActivityLevel.VeryActive => 1.9m,
                    _ => 1.2m
                };

                return Math.Round(BMR.Value * multiplier, 2);
            }
        }
    }
}