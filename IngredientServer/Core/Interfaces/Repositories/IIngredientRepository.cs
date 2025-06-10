using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IIngredientRepository
{
    Task<Ingredient> AddAsync(Ingredient ingredient);
    Task<Ingredient?> UpdateAsync(Ingredient ingredient);
    Task<bool> DeleteAsync(int id);
    Task<Ingredient?> GetByIdAsync(int id);
    Task<Ingredient?> GetByIdAndUserIdAsync(int id, int userId);
    Task<List<Ingredient>> GetAllAsync();
    Task<List<Ingredient>> GetByUserIdAsync(int userId);
    Task<List<Ingredient>> GetExpiringItemsAsync(int userId, int days = 7);
    Task<List<Ingredient>> GetExpiredItemsAsync(int userId);
    Task<List<Ingredient>> GetFilteredAsync(IngredientFilterDto filter);
    Task<List<Ingredient>> GetSortedAsync(int userId, IngredientSortDto sort);
    Task<List<Ingredient>> GetByCategoryAsync(int userId, IngredientCategory category);
    Task<int> CountByUserIdAsync(int userId);
    Task<bool> ExistsAsync(int id, int userId);
}