using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IngredientServer.Core.Entities;

public class UpdateUserProfileDto 
{
    [MaxLength(50)]
    public string? Username { get; set; }
    
    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }
    
    public Gender? Gender { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public decimal? TargetWeight { get; set; }
    
    public NutritionGoal? PrimaryNutritionGoal { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActivityLevel? ActivityLevel { get; set; }
    
    public bool? HasFoodAllergies { get; set; }
    
    [StringLength(1000)]
    public string? FoodAllergies { get; set; }
    
    [StringLength(1000)]
    public string? FoodPreferences { get; set; }
    
    public bool? EnableNotifications { get; set; }
    public bool? EnableMealReminders { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    [MinLength(6)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}