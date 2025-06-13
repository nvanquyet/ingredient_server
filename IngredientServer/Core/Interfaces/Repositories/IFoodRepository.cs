using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IFoodRepository : IBaseRepository<Food>
{
    Task<Food?> GetFoodDetailsAsync(int foodId, int userId);
}