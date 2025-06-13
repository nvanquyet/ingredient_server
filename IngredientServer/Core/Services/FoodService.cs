using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;

namespace IngredientServer.Core.Services;

public class FoodService : IFoodService
{
    private readonly IFoodRepository _foodRepository;

    public FoodService(IFoodRepository foodRepository)
    {
        _foodRepository = foodRepository;
    }

    public async Task<List<Food>> GetUserFoodsAsync(int userId)
    {
        return await _foodRepository.GetByUserIdAsync(userId);
    }

    public async Task<Food?> GetFoodByIdAsync(int foodId, int userId)
    {
        return await _foodRepository.GetByIdAndUserIdAsync(foodId, userId);
    }

    public async Task<Food> CreateFoodAsync(Food food)
    {
        food.CreatedAt = DateTime.UtcNow;
        food.UpdatedAt = DateTime.UtcNow;
        return await _foodRepository.CreateAsync(food);
    }

    public async Task<Food> UpdateFoodAsync(Food food)
    {
        food.UpdatedAt = DateTime.UtcNow;
        return await _foodRepository.UpdateAsync(food);
    }

    public async Task<bool> DeleteFoodAsync(int foodId, int userId)
    {
        var food = await _foodRepository.GetByIdAndUserIdAsync(foodId, userId);
        if (food == null) return false;
            
        return await _foodRepository.DeleteAsync(food);
    }

    public async Task<List<Food>> SearchFoodsAsync(int userId, string searchTerm)
    {
        return await _foodRepository.SearchAsync(userId, searchTerm);
    }

    public async Task<List<Food>> GetFoodsByCategoryAsync(int userId, FoodCategory category)
    {
        return await _foodRepository.GetByCategoryAsync(userId, category);
    }

    public async Task<List<Food>> GetFoodsByIngredientsAsync(int userId, List<int> ingredientIds)
    {
        return await _foodRepository.GetByIngredientsAsync(userId, ingredientIds);
    }
}