using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Ingredient;

namespace IngredientServer.Core.Interfaces.Repositories;


public interface IFoodRepository : IBaseRepository<Food>
{
    // Additional methods specific to Food, if needed
    Task<IEnumerable<Food>> GetByMealIdAsync(int mealId, int pageNumber = 1, int pageSize = 10);
}
public interface IFoodIngredientRepository : IBaseRepository<FoodIngredient>
{
    // Get FoodIngredients by FoodId
    Task<IEnumerable<FoodIngredient>> GetByFoodIdAsync(int foodId);
}