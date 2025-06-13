using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Services;

public interface IFoodService : IBaseService<Food>
{
    Task<Food?> GetFoodDetailsAsync(int foodId, int userId);
}