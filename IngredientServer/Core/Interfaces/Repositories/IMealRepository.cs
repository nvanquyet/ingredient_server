using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IMealRepository : IBaseRepository<Meal>
{
    // Lấy meals theo ngày
    Task<IEnumerable<Meal>> GetByDateAsync(string date, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Meal>> GetByDateAsync(DateTime date);
    // Lấy meal kèm foods
    Task<Meal> GetByIdWithFoodsAsync(int mealId);
}

public interface IMealFoodRepository : IBaseRepository<MealFood>
{
    // Lấy MealFoods theo MealId
    Task<IEnumerable<MealFood>> GetByMealIdAsync(int mealId);
}