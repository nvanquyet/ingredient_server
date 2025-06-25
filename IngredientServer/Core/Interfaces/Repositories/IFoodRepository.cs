using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IFoodRepository : IBaseRepository<Food> {
    // Lấy foods theo MealId
    Task<IEnumerable<Food>> GetByMealIdAsync(int mealId, int pageNumber = 1, int pageSize = 10);

    // Lấy food với ingredients
    Task<Food> GetByIdWithIngredientsAsync(int foodId);
}

public interface IFoodIngredientRepository : IBaseRepository<FoodIngredient>
{
    // Lấy FoodIngredients theo FoodId
    Task<IEnumerable<FoodIngredient>> GetByFoodIdAsync(int foodId);
}