using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class MealRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<Meal>(context, userContextService), IMealRepository
{
    public async Task<List<Meal>> GetByTimeRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Meal>()
            .Where(m => m.UserId == AuthenticatedUserId && m.MealDate >= startDate && m.MealDate <= endDate)
            .Include(m => m.MealFoods)
                .ThenInclude(mf => mf.Food)
            .OrderByDescending(m => m.MealDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Meal>> GetRecentMealsAsync(int days, int pageNumber = 1, int pageSize = 10)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);
        var endDate = DateTime.UtcNow.Date.AddDays(1); // Include today
        return await GetByTimeRangeAsync(startDate, endDate, pageNumber, pageSize);
    }

    public async Task<List<Meal>> GetByMealTypeAsync(MealType mealType, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Meal>()
            .Where(m => m.UserId == AuthenticatedUserId && m.MealType == mealType)
            .Include(m => m.MealFoods)
                .ThenInclude(mf => mf.Food)
            .OrderByDescending(m => m.MealDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Meal>> GetTodayMealsAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await Context.Set<Meal>()
            .Where(m => m.UserId == AuthenticatedUserId && m.MealDate.Date == today)
            .Include(m => m.MealFoods)
                .ThenInclude(mf => mf.Food)
            .OrderBy(m => m.MealType)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddFoodToMealAsync(int mealId, int foodId, decimal portionWeight)
    {
        // Verify meal ownership
        var meal = await GetByIdAsync(mealId);
        if (meal == null)
            throw new UnauthorizedAccessException("Meal not found or access denied.");

        // Verify food ownership
        var food = await Context.Set<Food>()
            .Where(f => f.Id == foodId && f.UserId == AuthenticatedUserId)
            .FirstOrDefaultAsync();
        if (food == null)
            throw new UnauthorizedAccessException("Food not found or access denied.");

        // Check if food is already in meal
        var existingMealFood = await Context.Set<MealFood>()
            .Where(mf => mf.MealId == mealId && mf.FoodId == foodId)
            .FirstOrDefaultAsync();

        if (existingMealFood != null)
        {
            // Update portion weight if already exists
            //existingMealFood.PortionWeight = portionWeight;
            Context.Set<MealFood>().Update(existingMealFood);
        }
        else
        {
            // Add new meal food
            var mealFood = new MealFood
            {
                FoodId = foodId,
                MealId = mealId
            };
            Context.Set<MealFood>().Add(mealFood);
        }

        await Context.SaveChangesAsync();
    }

    public async Task RemoveFoodFromMealAsync(int mealId, int foodId)
    {
        // Verify meal ownership first
        var meal = await GetByIdAsync(mealId);
        if (meal == null)
            throw new UnauthorizedAccessException("Meal not found or access denied.");

        var mealFood = await Context.Set<MealFood>()
            .Where(mf => mf.MealId == mealId && mf.FoodId == foodId)
            .FirstOrDefaultAsync();

        if (mealFood == null)
            throw new InvalidOperationException("Food not found in this meal.");

        Context.Set<MealFood>().Remove(mealFood);
        await Context.SaveChangesAsync();
    }

    public async Task<Meal?> GetMealDetailsAsync(int mealId)
    {
        return await Context.Set<Meal>()
            .Where(m => m.Id == mealId && m.UserId == AuthenticatedUserId)
            .Include(m => m.MealFoods)
                .ThenInclude(mf => mf.Food)
                    .ThenInclude(f => f.FoodIngredients)
                        .ThenInclude(fi => fi.Ingredient)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetUserMealCountAsync()
    {
        return await Context.Set<Meal>()
            .CountAsync(m => m.UserId == AuthenticatedUserId);
    }

    public async Task<List<Meal>> GetMealsByDateAsync(DateTime date, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Meal>()
            .Where(m => m.UserId == AuthenticatedUserId && m.MealDate.Date == date.Date)
            .Include(m => m.MealFoods)
                .ThenInclude(mf => mf.Food)
            .OrderBy(m => m.MealType)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }
}