using System.ComponentModel.DataAnnotations;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Entities
{
    public enum MealType
    {
        Breakfast,
        Lunch,
        Dinner,
        Snack,
        Other
    }

    public class Meal : BaseEntity
    {
        [Required]
        public MealType MealType { get; set; }

        [Required]
        public DateTime MealDate { get; set; }

        // THAY ĐỔI: ConsumedAt có thể nullable (chưa ăn)
        public DateTime? ConsumedAt { get; set; }
        
        // BỔ SUNG: Thông tin dinh dưỡng tổng hợp
        [Range(0, double.MaxValue)]
        public double TotalCalories { get; set; }
        
        [Range(0, double.MaxValue)]
        public double TotalProtein { get; set; }
        
        [Range(0, double.MaxValue)]
        public double TotalCarbs { get; set; }
        
        [Range(0, double.MaxValue)]
        public double TotalFat { get; set; }
        [Range(0, double.MaxValue)]
        public double TotalFiber { get; set; }
        
        // Navigation properties (GIỮ NGUYÊN)
        public User User { get; set; } = null!;
        public ICollection<MealFood> MealFoods { get; set; } = new List<MealFood>();
        
        // Computed properties
        public int FoodCount => MealFoods.Count;
        
        public void UpdateMeal(MealDto target)
        {
            this.MealType = target.MealType;
            this.MealDate = target.MealDate;
            this.ConsumedAt = target.ConsumedAt;
            this.UpdatedAt = target.UpdatedAt;
        }
        
        public MealDto ToDto()
        {
            return new MealDto
            {
                Id = this.Id,
                MealType = this.MealType,
                MealDate = this.MealDate,
                ConsumedAt = this.ConsumedAt,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }
    }
}