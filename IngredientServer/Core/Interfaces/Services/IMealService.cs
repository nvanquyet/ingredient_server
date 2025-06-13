using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Services;

public interface IMealService : IBaseService<Meal>
{
    Task<IEnumerable<Meal>> GetByTimeRangeAsync(int userId, DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Meal>> GetRecentMealsAsync(int userId, int days, int pageNumber = 1, int pageSize = 10);
    Task AddFoodToMealAsync(int mealId, int foodId, decimal portionWeight, int userId);
    Task RemoveFoodFromMealAsync(int mealId, int foodId, int userId);
    Task<Meal?> GetMealDetailsAsync(int mealId, int userId);
}