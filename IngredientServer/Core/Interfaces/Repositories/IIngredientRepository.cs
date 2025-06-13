using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IIngredientRepository : IBaseRepository<Ingredient>
{
    Task<Ingredient?> GetByIdAndUserIdAsync(int id, int userId);
    Task<List<Ingredient>> GetByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10);
    Task<List<Ingredient>> GetExpiringItemsAsync(int userId, int days = 7);
    Task<List<Ingredient>> GetExpiredItemsAsync(int userId);
    Task<List<Ingredient>> GetFilteredAsync(IngredientFilterDto filter, int pageNumber = 1, int pageSize = 10);
    Task<List<Ingredient>> GetSortedAsync(int userId, IngredientSortDto sort, int pageNumber = 1, int pageSize = 10);

    Task<List<Ingredient>> GetByCategoryAsync(int userId, IngredientCategory category, int pageNumber = 1,
        int pageSize = 10);

    Task<int> CountByUserIdAsync(int userId);
}