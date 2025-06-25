using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Interfaces.Services;

public interface IExternalApiService
{
    Task<IEnumerable<FoodSuggestionDto>> GetFoodSuggestionsAsync(FoodSuggestionRequestDto requestDto);
    Task<FoodRecipeDto> GetRecipeAsync(FoodRecipeRequestDto request);
}