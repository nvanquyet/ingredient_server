using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Exceptions;

namespace IngredientServer.Core.Services;

public class MealService : IMealService
{
    private readonly IMealRepository _mealRepository;

    public MealService(IMealRepository mealRepository)
    {
        _mealRepository = mealRepository;
    }

    public async Task<Meal?> GetByIdAsync(int id, int userId)
    {
        if (id <= 0 || userId <= 0)
            throw new ArgumentException("Invalid id or userId");

        return await _mealRepository.GetByIdAsync(id, userId);
    }

    public async Task<IEnumerable<Meal>> GetAllAsync(int userId, int pageNumber = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (pageNumber < 1)
            throw new ArgumentException("Invalid pageNumber");
        if (pageSize <= 0)
            throw new ArgumentException("Invalid pageSize");

        return await _mealRepository.GetAllAsync(userId, pageNumber, pageSize);
    }

    public async Task<Meal> AddAsync(Meal entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        if (entity.UserId <= 0)
            throw new ArgumentException("Invalid UserId");

        return await _mealRepository.AddAsync(entity);
    }

    public async Task<Meal> UpdateAsync(Meal entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        if (entity.Id <= 0 || entity.UserId <= 0)
            throw new ArgumentException("Invalid Id or UserId");

        var existing = await _mealRepository.GetByIdAsync(entity.Id, entity.UserId);
        if (existing == null)
            throw new NotFoundException($"Meal with ID {entity.Id} not found for user {entity.UserId}");

        return await _mealRepository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        if (id <= 0 || userId <= 0)
            throw new ArgumentException("Invalid id or userId");

        return await _mealRepository.DeleteAsync(id, userId);
    }

    public async Task<bool> ExistsAsync(int id, int userId)
    {
        if (id <= 0 || userId <= 0)
            throw new ArgumentException("Invalid id or userId");

        return await _mealRepository.ExistsAsync(id, userId);
    }

    public async Task<IEnumerable<Meal>> GetByTimeRangeAsync(int userId, DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before end date");
        if (pageNumber < 1)
            throw new ArgumentException("Invalid pageNumber");
        if (pageSize <= 0)
            throw new ArgumentException("Invalid pageSize");

        return await _mealRepository.GetByTimeRangeAsync(userId, startDate, endDate, pageNumber, pageSize);
    }

    public async Task<IEnumerable<Meal>> GetRecentMealsAsync(int userId, int days, int pageNumber = 1, int pageSize = 10)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid userId");
        if (days < 0)
            throw new ArgumentException("Invalid days");
        if (pageNumber < 1)
            throw new ArgumentException("Invalid pageNumber");
        if (pageSize <= 0)
            throw new ArgumentException("Invalid pageSize");

        return await _mealRepository.GetRecentMealsAsync(userId, days, pageNumber, pageSize);
    }

    public async Task AddFoodToMealAsync(int mealId, int foodId, decimal portionWeight, int userId)
    {
        if (mealId <= 0 || foodId <= 0 || userId <= 0)
            throw new ArgumentException("Invalid mealId, foodId, or userId");
        if (portionWeight <= 0)
            throw new ArgumentException("Portion weight must be positive");

        await _mealRepository.AddFoodToMealAsync(mealId, foodId, portionWeight, userId);
    }

    public async Task RemoveFoodFromMealAsync(int mealId, int foodId, int userId)
    {
        if (mealId <= 0 || foodId <= 0 || userId <= 0)
            throw new ArgumentException("Invalid mealId, foodId, or userId");

        await _mealRepository.RemoveFoodFromMealAsync(mealId, foodId, userId);
    }

    public async Task<Meal?> GetMealDetailsAsync(int mealId, int userId)
    {
        if (mealId <= 0 || userId <= 0)
            throw new ArgumentException("Invalid mealId or userId");

        var meal = await _mealRepository.GetMealDetailsAsync(mealId, userId);
        if (meal == null)
            throw new NotFoundException($"Meal with ID {mealId} not found for user {userId}");

        return meal;
    }
}