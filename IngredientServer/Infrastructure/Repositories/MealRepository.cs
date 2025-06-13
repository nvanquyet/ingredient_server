using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;
public class MealRepository(ApplicationDbContext context) : BaseRepository<Meal>(context), IMealRepository
{
    public async Task<List<Meal>> GetByTimeRangeAsync(int userId, DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Meal>()
            .Where(m => m.UserId == userId && m.ConsumedAt >= startDate && m.ConsumedAt <= endDate)
            .Include(m => m.MealFoods)
                .ThenInclude(mf => mf.Food)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Meal>> GetRecentMealsAsync(int userId, int days, int pageNumber = 1, int pageSize = 10)
    {
        var startDate = DateTime.Now.AddDays(-days);
        var endDate = DateTime.Now;
        return await GetByTimeRangeAsync(userId, startDate, endDate, pageNumber, pageSize);
    }

    public async Task AddFoodToMealAsync(int mealId, int foodId, decimal portionWeight, int userId)
    {
        var meal = await GetByIdAsync(mealId, userId);
        if (meal == null) throw new Exception("Meal not found");

        var food = await Context.Set<Food>()
            .Where(f => f.Id == foodId && f.UserId == userId)
            .FirstOrDefaultAsync();
        if (food == null) throw new Exception("Food not found");

        var mealFood = new MealFood
        {
            FoodId = foodId,
            MealId = mealId,
            Meal = meal,
            Food = food
        };

        Context.Set<MealFood>().Add(mealFood);
        await Context.SaveChangesAsync();
    }

    public async Task RemoveFoodFromMealAsync(int mealId, int foodId, int userId)
    {
        var mealFood = await Context.Set<MealFood>()
            .Where(mf => mf.MealId == mealId && mf.FoodId == foodId && mf.Meal.UserId == userId)
            .FirstOrDefaultAsync();
        if (mealFood == null) throw new Exception("MealFood not found");

        Context.Set<MealFood>().Remove(mealFood);
        await Context.SaveChangesAsync();
    }

    public async Task<Meal?> GetMealDetailsAsync(int mealId, int userId)
    {
        return await Context.Set<Meal>()
            .Where(m => m.Id == mealId && m.UserId == userId)
            .Include(m => m.MealFoods)
                .ThenInclude(mf => mf.Food)
            .FirstOrDefaultAsync();
    }
}