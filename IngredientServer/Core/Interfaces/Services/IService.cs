using IngredientServer.Core.Entities;
using IngredientServer.Utils.DTOs.Ingredient;
using IngredientServer.Utils.DTOs.Meal;

namespace IngredientServer.Core.Interfaces.Services;

public interface IFoodService
{
    Task<Food> CreateFoodAsync(CreateFoodDto dto);
    Task<Food> UpdateFoodAsync(int foodId, UpdateFoodDto dto);
    Task<bool> DeleteFoodAsync(int foodId);
}

public interface IIngredientService
{
    Task<Ingredient> CreateIngredientAsync(CreateIngredientDto dto);
    Task<Ingredient> UpdateIngredientAsync(int ingredientId, UpdateIngredientDto dto);
    Task<bool> DeleteIngredientAsync(int ingredientId);
    
    Task<IngredientSearchResultDto> GetAllAsync(IngredientFilterDto filter);
}

public interface IMealService
{
    Task<MealWithFoodsDto> GetByIdAsync(int mealId);
    Task<IEnumerable<MealWithFoodsDto>> GetByDateAsync(string date, int pageNumber = 1, int pageSize = 10);
    Task<Meal> CreateMealAsync(MealType mealType, DateTime mealDate);
    Task<Meal> UpdateMealAsync(int mealId, MealDto updateMealDto);
    Task<bool> DeleteMealAsync(int mealId);
}