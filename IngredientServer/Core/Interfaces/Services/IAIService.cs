using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Interfaces.Services;

// Interface
public interface IAIService
{
    Task<List<FoodSuggestionResponseDto>> GetSuggestionsAsync(FoodSuggestionRequestDto requestDto, List<FoodIngredientDto> ingredients, CancellationToken cancellationToken = default);
    Task<FoodDataResponseDto> GetRecipeSuggestionsAsync(FoodRecipeRequestDto recipeRequest, CancellationToken cancellationToken = default);
    
    Task<List<int>> GetTargetDailyNutritionAsync(UserInformationDto userInformation, CancellationToken cancellationToken = default);
    Task<List<int>> GetTargetWeeklyNutritionAsync(UserInformationDto userInformation, CancellationToken cancellationToken = default);
    Task<List<int>> GetTargetOverviewNutritionAsync(UserInformationDto userInformation, int dayAmount, CancellationToken cancellationToken = default);
}