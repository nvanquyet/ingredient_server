using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs.Auth;

public class UpdateUserProfileDto
{
    [MaxLength(50)]
    public string? Username { get; set; }
    
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmNewPassword { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    public Gender? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal? Height { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal? Weight { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal? TargetWeight { get; set; }
    public NutritionGoal? PrimaryNutritionGoal { get; set; }
    public ActivityLevel? ActivityLevel { get; set; }
    public bool? HasFoodAllergies { get; set; }
    [StringLength(1000)]
    public string? FoodAllergies { get; set; }
    [StringLength(1000)]
    public string? FoodPreferences { get; set; }
    public bool? EnableNotifications { get; set; }
    public bool? EnableMealReminders { get; set; }
}