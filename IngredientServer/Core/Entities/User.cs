using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Auth;

namespace IngredientServer.Core.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NutritionGoal
    {
        Balanced = 0,           // Cân bằng dinh dưỡng
        WeightLoss = 1,         // Giảm cân
        WeightGain = 2,         // Tăng cân
        MuscleGain = 3,         // Tăng cơ
        LowCarb = 4,            // Ít carb
        HighProtein = 5,        // Nhiều protein
        Vegetarian = 6,         // Ăn chay
        Vegan = 7,              // Ăn thuần chay
        Keto = 8,               // Chế độ Keto
        Mediterranean = 9,      // Địa Trung Hải
        Paleo = 10,              // Paleo
        LowSodium = 11,          // Ít muối
        DiabeticFriendly = 12,   // Phù hợp tiểu đường
        HeartHealthy = 13,       // Bảo vệ tim mạch
        AntiInflammatory = 14,   // Chống viêm
        Other = 15              // Khác
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActivityLevel
    {
        Sedentary = 0,      // Ít vận động
        Light = 1,          // Vận động nhẹ
        Moderate = 2,       // Vận động vừa
        Active = 3,         // Vận động nhiều
        VeryActive = 4    // Vận động rất nhiều
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2,
        PreferNotToSay = 3
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
        
        //Ràng buộc dinh dưỡng
        public UserNutritionTargets? NutritionTargets { get; set; }

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

        public void UpdateUserProfile(UserProfileDto targetData)
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
            this.UpdatedAt = DateTime.UtcNow;
        }

        public UserProfileDto ToDto()
        {
            return new UserProfileDto
            {
                Id = this.Id,
                Username = this.Username,
                Email = this.Email,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Gender = this.gender,
                DateOfBirth = this.DateOfBirth,
                Height = this.Height,
                Weight = this.Weight,
                TargetWeight = this.TargetWeight,
                PrimaryNutritionGoal = this.PrimaryNutritionGoal,
                ActivityLevel = this.ActivityLevel,
                HasFoodAllergies = this.HasFoodAllergies,
                FoodAllergies = this.FoodAllergies,
                FoodPreferences = this.FoodPreferences,
                EnableNotifications = this.EnableNotifications,
                EnableMealReminders = this.EnableMealReminders,
            };
        }

        public override void NormalizeDateTimes()
        {
            base.NormalizeDateTimes();
            if (DateOfBirth.HasValue && DateOfBirth.Value.Kind != DateTimeKind.Utc)
            {
                DateOfBirth = DateTime.SpecifyKind(DateOfBirth.Value, DateTimeKind.Utc);
            }
        }
    }
}