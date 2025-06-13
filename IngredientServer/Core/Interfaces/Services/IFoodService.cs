using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Services;

public interface IFoodService
{
    Task<List<Food>> GetUserFoodsAsync(int userId);
    Task<Food?> GetFoodByIdAsync(int foodId, int userId);
    Task<Food> CreateFoodAsync(Food food);
    Task<Food> UpdateFoodAsync(Food food);
    Task<bool> DeleteFoodAsync(int foodId, int userId);
    Task<List<Food>> SearchFoodsAsync(int userId, string searchTerm);
    Task<List<Food>> GetFoodsByCategoryAsync(int userId, FoodCategory category);
    Task<List<Food>> GetFoodsByIngredientsAsync(int userId, List<int> ingredientIds);
}