using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IngredientServer.Utils.DTOs.Auth;

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

        // Thông tin cá nhân
        public DateTime? DateOfBirth { get; set; }
        public Gender? gender { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Height { get; set; } // cm
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; } // kg
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TargetWeight { get; set; } // kg

        // Mục tiêu dinh dưỡng
        public NutritionGoal PrimaryNutritionGoal { get; set; } = NutritionGoal.Balanced;
        public ActivityLevel ActivityLevel { get; set; } = ActivityLevel.Sedentary;

        // Ràng buộc ăn uống
        public bool HasFoodAllergies { get; set; } = false;
        [StringLength(1000)]
        public string? FoodAllergies { get; set; }

        // Tùy chọn (có thể thêm sau)
        [StringLength(1000)]
        public string? FoodPreferences { get; set; }
        public bool EnableNotifications { get; set; } = true;
        public bool EnableMealReminders { get; set; } = true;

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

        public decimal? BMR
        {
            get
            {
                if (!Weight.HasValue || !Height.HasValue || !Age.HasValue || !gender.HasValue)
                    return null;

                decimal bmr = gender == Gender.Male
                    ? (10 * Weight.Value) + (6.25m * Height.Value) - (5 * Age.Value) + 5
                    : (10 * Weight.Value) + (6.25m * Height.Value) - (5 * Age.Value) - 161;

                return Math.Round(bmr, 2);
            }
        }

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

        public void UpdateUserProfile(UpdateUserProfileDto targetData)
        {
            if (targetData.FirstName != null)
                this.FirstName = targetData.FirstName;
            if (targetData.LastName != null)
                this.LastName = targetData.LastName;
            if (targetData.Email != null)
                this.Email = targetData.Email;
            if (targetData.Username != null)
                this.Username = targetData.Username; 
            if (targetData.Gender.HasValue)
                this.gender = targetData.Gender;
            if (targetData.DateOfBirth.HasValue)
                this.DateOfBirth = targetData.DateOfBirth;
            if (targetData.Height.HasValue)
                this.Height = targetData.Height;
            if (targetData.Weight.HasValue)
                this.Weight = targetData.Weight;
            if (targetData.TargetWeight.HasValue)
                this.TargetWeight = targetData.TargetWeight;
            if (targetData.PrimaryNutritionGoal.HasValue)
                this.PrimaryNutritionGoal = targetData.PrimaryNutritionGoal.Value;
            if (targetData.ActivityLevel.HasValue)
                this.ActivityLevel = targetData.ActivityLevel.Value;
            if (targetData.HasFoodAllergies.HasValue)
                this.HasFoodAllergies = targetData.HasFoodAllergies.Value;
            if (targetData.FoodAllergies != null)
                this.FoodAllergies = targetData.FoodAllergies;
            if (targetData.FoodPreferences != null)
                this.FoodPreferences = targetData.FoodPreferences;
            if (targetData.EnableNotifications.HasValue)
                this.EnableNotifications = targetData.EnableNotifications.Value;
            if (targetData.EnableMealReminders.HasValue)
                this.EnableMealReminders = targetData.EnableMealReminders.Value;
            
            if (!string.IsNullOrEmpty(targetData.CurrentPassword) && 
                !string.IsNullOrEmpty(targetData.NewPassword) && 
                !string.IsNullOrEmpty(targetData.ConfirmNewPassword))
            {
                if (targetData.CurrentPassword != PasswordHash)
                {
                    throw new Exception("Current password is incorrect");
                }

                if (targetData.NewPassword != targetData.ConfirmNewPassword)
                {
                    throw new Exception("New password and confirmation do not match");
                }

                if (targetData.NewPassword.Length < 8)
                {
                    throw new Exception("New password must be at least 8 characters long");
                }

                this.PasswordHash = BCrypt.Net.BCrypt.HashPassword(targetData.NewPassword);
            }

            this.UpdatedAt = DateTime.UtcNow;
        }
    }
}