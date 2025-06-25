using IngredientServer.Utils.DTOs;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.Core.Interfaces.Services;

public interface IExternalApiService
{
    Task<IEnumerable<FoodSuggestionDto>> GetFoodSuggestionsAsync(FoodSuggestionRequest request);
    Task<RecipeDto> GetRecipeAsync(GetRecipeRequestDto request);
}