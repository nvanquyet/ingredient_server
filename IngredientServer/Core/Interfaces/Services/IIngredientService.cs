using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.Core.Interfaces.Services;

public interface IIngredientService : IBaseService<Ingredient>
{
    Task<Ingredient?> GetByIdAndUserIdAsync(int id, int userId);
    Task<IEnumerable<Ingredient>> GetByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Ingredient>> GetExpiringItemsAsync(int userId, int days = 7);
    Task<IEnumerable<Ingredient>> GetExpiredItemsAsync(int userId);
    Task<IEnumerable<Ingredient>> GetFilteredAsync(IngredientFilterDto filter, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Ingredient>> GetSortedAsync(int userId, IngredientSortDto sort, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Ingredient>> GetByCategoryAsync(int userId, IngredientCategory category, int pageNumber = 1, int pageSize = 10);
    Task<int> CountByUserIdAsync(int userId);
}