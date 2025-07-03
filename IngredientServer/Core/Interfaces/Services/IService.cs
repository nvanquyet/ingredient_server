using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;
namespace IngredientServer.Core.Interfaces.Services;

public interface INutritionService
{
    Task<DailyNutritionSummaryDto> GetDailyNutritionSummaryAsync(UserNutritionRequestDto userNutritionRequestDto, bool usingAIAssistant = false);
    Task<WeeklyNutritionSummaryDto> GetWeeklyNutritionSummaryAsync(UserNutritionRequestDto userNutritionRequestDto);
    Task<OverviewNutritionSummaryDto> GetOverviewNutritionSummaryAsync(UserInformationDto userInformation);
}


public interface IFoodService
{
    Task<FoodDataResponseDto> CreateFoodAsync(CreateFoodRequestDto dataDto);
    Task<FoodDataResponseDto> UpdateFoodAsync(UpdateFoodRequestDto dto);
    Task<bool> DeleteFoodAsync(int foodId);
    Task<List<FoodSuggestionResponseDto>> GetSuggestionsAsync(FoodSuggestionRequestDto requestDto);
    Task<FoodDataResponseDto> GetRecipeSuggestionsAsync(FoodRecipeRequestDto recipeRequest);
    Task<FoodDataResponseDto> GetFoodByIdAsync(int id);
}

public interface IIngredientService
{
    Task<IngredientDataResponseDto> CreateIngredientAsync(CreateIngredientRequestDto dto);
    Task<IngredientDataResponseDto> UpdateIngredientAsync(UpdateIngredientRequestDto dto);
    Task<bool> DeleteIngredientAsync(int ingredientId);
    
    Task<IngredientSearchResultDto> GetAllAsync(IngredientFilterDto filter);
    Task<IngredientDataResponseDto> GetIngredientByIdAsync(int id);
}