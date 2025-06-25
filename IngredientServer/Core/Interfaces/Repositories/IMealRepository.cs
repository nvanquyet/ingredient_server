using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Meal;

namespace IngredientServer.Core.Interfaces.Repositories;


public interface IMealRepository : IBaseRepository<Meal>
{
    // Get meals by date
    Task<IEnumerable<Meal>> GetByDateAsync(string date, int pageNumber = 1, int pageSize = 10);
}

public interface IMealFoodRepository : IBaseRepository<MealFood>
{
    // Get MealFoods by MealId
    Task<IEnumerable<MealFood>> GetByMealIdAsync(int mealId);
}