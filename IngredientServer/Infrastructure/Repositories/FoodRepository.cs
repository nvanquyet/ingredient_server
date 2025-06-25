using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class FoodRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<Food>(context, userContextService), IFoodRepository
{
    // Override GetAllAsync to include related FoodIngredients
    public override async Task<IEnumerable<Food>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<Food>()
            .Where(e => e.UserId == AuthenticatedUserId)
            .Include(f => f.FoodIngredients)
            .ThenInclude(fi => fi.Ingredient)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // Override GetByIdAsync to include related FoodIngredients
    public override async Task<Food?> GetByIdAsync(int id)
    {
        return await Context.Set<Food>()
            .Where(e => e.Id == id && e.UserId == AuthenticatedUserId)
            .Include(f => f.FoodIngredients)
            .ThenInclude(fi => fi.Ingredient)
            .FirstOrDefaultAsync();
    }

    // Get foods by meal ID
    public async Task<IEnumerable<Food>> GetByMealIdAsync(int mealId, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<MealFood>()
            .Where(mf => mf.MealId == mealId && mf.Meal.UserId == AuthenticatedUserId)
            .Include(mf => mf.Food)
            .ThenInclude(f => f.FoodIngredients)
            .ThenInclude(fi => fi.Ingredient)
            .Select(mf => mf.Food)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}