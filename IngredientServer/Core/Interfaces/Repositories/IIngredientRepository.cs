using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IIngredientRepository : IBaseRepository<Ingredient>
{
    Task<List<Ingredient>> GetAllUserIngredientsAsync(int pageNumber = 1, int pageSize = 10);
    Task<List<Ingredient>> GetExpiringItemsAsync(int days = 7);
    Task<List<Ingredient>> GetExpiredItemsAsync();
    Task<List<Ingredient>> GetFilteredAsync(IngredientFilterDto filter, int pageNumber = 1, int pageSize = 10);
    Task<List<Ingredient>> GetSortedAsync(IngredientSortDto sort, int pageNumber = 1, int pageSize = 10);
    Task<List<Ingredient>> GetByCategoryAsync(IngredientCategory category, int pageNumber = 1, int pageSize = 10);
    Task<int> GetUserIngredientCountAsync();
}