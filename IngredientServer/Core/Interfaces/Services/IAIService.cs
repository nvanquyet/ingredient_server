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
    Task<FoodAnalysticResponseDto> GetFoodAnalysticAsync(FoodAnalysticRequestDto request, CancellationToken cancellationToken = default);
    Task<IngredientAnalysticResponseDto> GetIngredientAnalysticAsync(IngredientAnalysticRequestDto request, CancellationToken cancellationToken = default);
    
}