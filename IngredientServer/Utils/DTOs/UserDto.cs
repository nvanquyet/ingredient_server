using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IngredientServer.Core.Entities;

namespace IngredientServer.Utils.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; } 
}

public class UserInformationDto
{
    public Gender? Gender { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public decimal? TargetWeight { get; set; }
    
    public NutritionGoal? PrimaryNutritionGoal { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActivityLevel? ActivityLevel { get; set; }
}


public class LoginDto
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}


public class RegisterDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;

    //[Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    //[Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    //[Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;
}

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