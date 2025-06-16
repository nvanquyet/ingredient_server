using IngredientServer.Core.Entities;

namespace IngredientServer.Core.Interfaces.Repositories;

public interface IFoodRepository : IBaseRepository<Food>
{
    Task<Food?> GetFoodDetailsAsync(int foodId);
    Task<List<Food>> GetAllUserFoodsAsync(int pageNumber = 1, int pageSize = 10);
    Task<List<Food>> GetByFoodCategoryAsync(FoodCategory category, int pageNumber = 1, int pageSize = 10);
    Task<List<Food>> SearchFoodsAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
    Task<List<Food>> GetFoodsByPreparationTimeAsync(int maxMinutes, int pageNumber = 1, int pageSize = 10);
    Task<List<Food>> GetRecentlyCreatedFoodsAsync(int days = 7, int pageNumber = 1, int pageSize = 10);
    Task AddIngredientToFoodAsync(int foodId, int ingredientId, decimal quantity);
    Task RemoveIngredientFromFoodAsync(int foodId, int ingredientId);
    Task<int> GetUserFoodCountAsync();
    Task<List<Food>> GetFoodsWithIngredientAsync(int ingredientId, int pageNumber = 1, int pageSize = 10);
}