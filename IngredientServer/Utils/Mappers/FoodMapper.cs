using IngredientServer.Core.Entities;
using IngredientServer.Core.Helpers;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Utils.Mappers;

/// <summary>
/// Extension methods for mapping Food Entity to DTOs
/// </summary>
public static class FoodMapper
{
    /// <summary>
    /// Maps Food entity to FoodDataResponseDto
    /// </summary>
    public static FoodDataResponseDto ToDto(this Food food)
    {
        if (food == null)
            throw new ArgumentNullException(nameof(food));

        var mealFood = food.MealFoods.FirstOrDefault();
        
        var dto = new FoodDataResponseDto
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            ImageUrl = food.ImageUrl,
            PreparationTimeMinutes = food.PreparationTimeMinutes,
            CookingTimeMinutes = food.CookingTimeMinutes,
            Calories = food.Calories,
            Protein = food.Protein,
            Carbohydrates = food.Carbohydrates,
            Fat = food.Fat,
            Fiber = food.Fiber,
            Instructions = food.Instructions,
            Tips = food.Tips,
            DifficultyLevel = food.DifficultyLevel,
            ConsumedAt = DateTimeHelper.NormalizeToUtc(food.ConsumedAt),
            MealType = mealFood?.Meal.MealType ?? MealType.Breakfast,
            MealDate = mealFood?.Meal.MealDate ?? DateTimeHelper.UtcNow,
            Ingredients = food.FoodIngredients.Select(fi => new FoodIngredientDto
            {
                IngredientId = fi.IngredientId,
                Quantity = fi.Quantity,
                Unit = fi.Unit,
                IngredientName = fi.Ingredient?.Name ?? string.Empty
            }).ToList()
        };
        
        dto.NormalizeConsumedAt();
        return dto;
    }

    /// <summary>
    /// Maps collection of Food entities to DTOs
    /// </summary>
    public static IEnumerable<FoodDataResponseDto> ToDto(this IEnumerable<Food> foods)
    {
        return foods.Select(f => f.ToDto());
    }
}

