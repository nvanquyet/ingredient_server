using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IFoodRepository : IBaseRepository<Food>
{
    Task<List<Food>> GetByUserIdAsync(int userId);
    Task<Food?> GetByIdAndUserIdAsync(int foodId, int userId);
    Task<List<Food>> SearchAsync(int userId, string searchTerm);
    Task<List<Food>> GetByCategoryAsync(int userId, FoodCategory category);
    Task<List<Food>> GetByIngredientsAsync(int userId, List<int> ingredientIds);
}