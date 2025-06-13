using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IMealRepository : IBaseRepository<Meal>
{
    Task<List<Meal>> GetByTimeRangeAsync(int userId, DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10);
    Task<List<Meal>> GetRecentMealsAsync(int userId, int days, int pageNumber = 1, int pageSize = 10);
    Task AddFoodToMealAsync(int mealId, int foodId, decimal portionWeight, int userId);
    Task RemoveFoodFromMealAsync(int mealId, int foodId, int userId);
    Task<Meal?> GetMealDetailsAsync(int mealId, int userId);
}