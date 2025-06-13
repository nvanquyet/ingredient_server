using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;

namespace IngredientServer.Core.Services;
public class IngredientService : IIngredientService
{
    private readonly IIngredientRepository _ingredientRepository;

    public IngredientService(IIngredientRepository ingredientRepository)
    {
        _ingredientRepository = ingredientRepository;
    }

    public async Task<List<Ingredient>> GetUserIngredientsAsync(int userId)
    {
        return await _ingredientRepository.GetByUserIdAsync(userId);
    }

    public async Task<List<Ingredient>> GetIngredientsByIdsAsync(List<int> ingredientIds, int userId)
    {
        return await _ingredientRepository.GetByIdsAndUserIdAsync(ingredientIds, userId);
    }

    public async Task<List<Ingredient>> GetExpiringIngredientsAsync(int userId, int daysUntilExpiry)
    {
        var expiryDate = DateTime.Now.Date.AddDays(daysUntilExpiry);
        return await _ingredientRepository.GetExpiringIngredientsAsync(userId, expiryDate);
    }

    public async Task<Ingredient?> GetIngredientByIdAsync(int ingredientId, int userId)
    {
        return await _ingredientRepository.GetByIdAndUserIdAsync(ingredientId, userId);
    }

    public async Task<Ingredient> CreateIngredientAsync(Ingredient ingredient)
    {
        ingredient.CreatedAt = DateTime.UtcNow;
        ingredient.UpdatedAt = DateTime.UtcNow;
        return await _ingredientRepository.CreateAsync(ingredient);
    }

    public async Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient)
    {
        ingredient.UpdatedAt = DateTime.UtcNow;
        return await _ingredientRepository.UpdateAsync(ingredient);
    }

    public async Task<bool> DeleteIngredientAsync(int ingredientId, int userId)
    {
        var ingredient = await _ingredientRepository.GetByIdAndUserIdAsync(ingredientId, userId);
        if (ingredient == null) return false;
        
        return await _ingredientRepository.DeleteAsync(ingredient);
    }

    public async Task<List<Ingredient>> SearchIngredientsAsync(int userId, string searchTerm)
    {
        return await _ingredientRepository.SearchAsync(userId, searchTerm);
    }

    public async Task<List<Ingredient>> GetIngredientsByCategoryAsync(int userId, IngredientCategory category)
    {
        return await _ingredientRepository.GetByCategoryAsync(userId, category);
    }
}
