using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Services;
public interface IIngredientService
{
    Task<List<Ingredient>> GetUserIngredientsAsync(int userId);
    Task<List<Ingredient>> GetIngredientsByIdsAsync(List<int> ingredientIds, int userId);
    Task<List<Ingredient>> GetExpiringIngredientsAsync(int userId, int daysUntilExpiry);
    Task<Ingredient?> GetIngredientByIdAsync(int ingredientId, int userId);
    Task<Ingredient> CreateIngredientAsync(Ingredient ingredient);
    Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient);
    Task<bool> DeleteIngredientAsync(int ingredientId, int userId);
    Task<List<Ingredient>> SearchIngredientsAsync(int userId, string searchTerm);
    Task<List<Ingredient>> GetIngredientsByCategoryAsync(int userId, IngredientCategory category);
}