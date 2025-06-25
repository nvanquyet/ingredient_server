using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using IngredientServer.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class MealFoodRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<MealFood>(context, userContextService), IMealFoodRepository
{
    public async Task<IEnumerable<MealFood>> GetByMealIdAsync(int mealId)
    {
        return await Context.Set<MealFood>()
            .Include(mf => mf.Food)
            .Where(mf => mf.MealId == mealId && mf.UserId == AuthenticatedUserId)
            .ToListAsync();
    }
}