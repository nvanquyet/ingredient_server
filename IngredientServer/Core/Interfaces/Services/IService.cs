using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
namespace IngredientServer.Core.Interfaces.Services;

public interface INutritionService
{
    Task<DailyNutritionSummaryDto> GetDailyNutritionSummaryAsync(DateTime date, UserInformationDto userInformation, bool usingAIAssistant = false);
    Task<WeeklyNutritionSummaryDto> GetWeeklyNutritionSummaryAsync(DateTime startDate, DateTime endDate, UserInformationDto userInformation);
    Task<OverviewNutritionSummaryDto> GetOverviewNutritionSummaryAsync(UserInformationDto userInformation);
}


public interface IFoodService
{
    Task<Food> CreateFoodAsync(FoodDataDto dataDto);
    Task<Food> UpdateFoodAsync(int foodId, FoodDataDto dto);
    Task<bool> DeleteFoodAsync(int foodId);
    Task<List<FoodSuggestionDto>> GetSuggestionsAsync(FoodSuggestionRequestDto requestDto);
    Task<FoodDataDto> GetRecipeSuggestionsAsync(FoodRecipeRequestDto recipeRequest);
    Task<FoodDataDto> GetFoodByIdAsync(int id);
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
    Task<MealDto> GetByIdAsync(int mealId);
    Task<IEnumerable<MealDto>> GetByDateAsync(string date);
    Task<MealDto> CreateMealAsync(MealType mealType, DateTime mealDate);
    Task<MealDto> UpdateMealAsync(int mealId, MealDto updateMealDto);
    Task<bool> DeleteMealAsync(int mealId);
}