using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
namespace IngredientServer.Core.Interfaces.Services;

public interface INutritionService
{
    Task<DailyNutritionSummaryDto> GetDailyNutritionSummaryAsync(int userId, DateTime date);
    Task<WeeklyNutritionSummaryDto> GetWeeklyNutritionSummaryAsync(int userId, DateTime startDate, DateTime endDate);
    Task<TotalNutritionSummaryDto> GetTotalNutritionSummaryAsync(int userId);
}


public interface IFoodService
{
    Task<Food> CreateFoodAsync(FoodDataDto dataDto);
    Task<Food> UpdateFoodAsync(int foodId, FoodDataDto dto);
    Task<bool> DeleteFoodAsync(int foodId);
    Task<List<FoodSuggestionDto>> GetSuggestionsAsync(FoodSuggestionRequestDto requestDto);
    Task<FoodRecipeDto> GetRecipeSuggestionsAsync(FoodRecipeRequestDto recipeRequest);
    Task<FoodDto> GetFoodByIdAsync(int id);
}

public interface IIngredientService
{
    Task<IngredientDto> CreateIngredientAsync(IngredientDataDto dataDto);
    Task<IngredientDto> UpdateIngredientAsync(int ingredientId, IngredientDataDto dto);
    Task<bool> DeleteIngredientAsync(int ingredientId);
    
    Task<IngredientSearchResultDto> GetAllAsync(IngredientFilterDto filter);
    Task<IngredientDto> GetIngredientByIdAsync(int id);
}

public interface IMealService
{
    Task<MealWithFoodsDto> GetByIdAsync(int mealId);
    Task<IEnumerable<MealWithFoodsDto>> GetByDateAsync(string date);
    Task<MealDto> CreateMealAsync(MealType mealType, DateTime mealDate);
    Task<MealDto> UpdateMealAsync(int mealId, MealDto updateMealDto);
    Task<bool> DeleteMealAsync(int mealId);
}