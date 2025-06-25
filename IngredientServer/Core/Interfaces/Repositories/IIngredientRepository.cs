using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IIngredientRepository : IBaseRepository<Ingredient>
{
    // Support filtering and pagination for ingredients
    Task<IEnumerable<Ingredient>> GetAllAsync(int pageNumber = 1, int pageSize = 10, IngredientFilterDto? filter = null);
}