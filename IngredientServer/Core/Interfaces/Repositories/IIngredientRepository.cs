using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IIngredientRepository : IBaseRepository<Ingredient>
{
    // Support filtering and pagination for ingredients
    Task<IngredientSearchResultDto> GetByFilterAsync(IngredientFilterDto? filter = null);
}