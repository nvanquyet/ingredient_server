using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IMealRepository : IBaseRepository<Meal>
{
    Task<List<Meal>> GetByTimeRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10);
    Task<List<Meal>> GetRecentMealsAsync(int days, int pageNumber = 1, int pageSize = 10);
    Task<List<Meal>> GetByMealTypeAsync(MealType mealType, int pageNumber = 1, int pageSize = 10);
    Task<List<Meal>> GetTodayMealsAsync();
    Task AddFoodToMealAsync(int mealId, int foodId, decimal portionWeight);
    Task RemoveFoodFromMealAsync(int mealId, int foodId);
    Task<Meal?> GetMealDetailsAsync(int mealId);
    Task<int> GetUserMealCountAsync();
    Task<List<Meal>> GetMealsByDateAsync(DateTime date, int pageNumber = 1, int pageSize = 10);
}