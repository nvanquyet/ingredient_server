using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Utils.DTOs.Entity;

namespace IngredientServer.Core.Services;

public class MealService(
    IMealRepository mealRepository,
    IMealFoodRepository mealFoodRepository,
    IUserContextService userContextService)
    : IMealService
{
    private readonly IMealFoodRepository _mealFoodRepository = mealFoodRepository;

    public async Task<MealWithFoodsDto> GetByIdAsync(int mealId)
    {
        var meal = await mealRepository.GetByIdWithFoodsAsync(mealId);
        return new MealWithFoodsDto
        {
            Id = meal.Id,
            MealType = meal.MealType,
            MealDate = meal.MealDate,
            Foods = meal.MealFoods.Select(mf => new FoodDto
            {
                Id = mf.Food.Id,
                Name = mf.Food.Name,
                Calories = mf.Food.Calories,
                Protein = mf.Food.Protein,
                Carbs = mf.Food.Carbs,
                Fat = mf.Food.Fat,
                Quantity = mf.Food.Quantity
            }).ToList()
        };
    }

    public async Task<IEnumerable<MealWithFoodsDto>> GetByDateAsync(string date)
    {
        var meals = await mealRepository.GetByDateAsync(date);
        return meals.Select(m => new MealWithFoodsDto
        {
            Id = m.Id,
            MealType = m.MealType,
            MealDate = m.MealDate,
            Foods = m.MealFoods.Select(mf => new FoodDto
            {
                Id = mf.Food.Id,
                Name = mf.Food.Name,
                Calories = mf.Food.Calories,
                Protein = mf.Food.Protein,
                Carbs = mf.Food.Carbs,
                Fat = mf.Food.Fat,
                Quantity = mf.Food.Quantity
            }).ToList()
        });
    }

    public async Task<MealDto> CreateMealAsync(MealType mealType, DateTime mealDate)
    {
        var meal = new Meal
        {
            MealType = mealType,
            MealDate = mealDate,
            UserId = userContextService.GetAuthenticatedUserId()
        };

        var savedMeal = await mealRepository.AddAsync(meal);
        return savedMeal.ToDto();
    }

    public async Task<MealDto> UpdateMealAsync(int mealId, MealDto updateMealDto)
    {
        var meal = await mealRepository.GetByIdAsync(mealId);
        if (meal == null)
        {
            throw new UnauthorizedAccessException("Meal not found or access denied.");
        }
        meal.UpdateMeal(updateMealDto);
        var updatedMeal = await mealRepository.UpdateAsync(meal);
        return updatedMeal.ToDto();
    }

    public async Task<bool> DeleteMealAsync(int mealId)
    {
        //Current version cannot delete meals, only update them
        return false;
    }
}