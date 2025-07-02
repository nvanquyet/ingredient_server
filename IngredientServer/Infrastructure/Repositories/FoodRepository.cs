using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using IngredientServer.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class FoodRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<Food>(context, userContextService), IFoodRepository
{
    public async Task<IEnumerable<Food>> GetByMealIdAsync(int mealId, int pageNumber = 1, int pageSize = 10)
    {
        return await Context.Set<MealFood>()
            .Include(mf => mf.Food)
            .Where(mf => mf.MealId == mealId && mf.UserId == AuthenticatedUserId)
            .Select(mf => mf.Food)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Food> GetByIdWithIngredientsAsync(int foodId)
    {
        var food = await Context.Set<Food>()
            .Include(f => f.FoodIngredients)
            .ThenInclude(fi => fi.Ingredient)
            .Where(f => f.Id == foodId && f.UserId == AuthenticatedUserId)
            .FirstOrDefaultAsync();

        if (food == null)
        {
            throw new UnauthorizedAccessException("Food not found or access denied.");
        }

        return food;
    }

    public override async Task<Food?> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Invalid food ID", nameof(id));
        }

        var food = await Context.Set<Food>()
            .Include(f => f.FoodIngredients)
            .ThenInclude(fi => fi.Ingredient)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == AuthenticatedUserId);

        if (food == null)
        {
            throw new UnauthorizedAccessException("Food not found or access denied.");
        }

        return food;
    }

}