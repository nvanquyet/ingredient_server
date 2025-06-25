using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;


public class MealFoodRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<MealFood>(context, userContextService), IMealFoodRepository
{
    // Get MealFoods by MealId
    public async Task<IEnumerable<MealFood>> GetByMealIdAsync(int mealId)
    {
        return await Context.Set<MealFood>()
            .Where(mf => mf.MealId == mealId && mf.Meal.UserId == AuthenticatedUserId)
            .Include(mf => mf.Food)
            .ToListAsync();
    }
}