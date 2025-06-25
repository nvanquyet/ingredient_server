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