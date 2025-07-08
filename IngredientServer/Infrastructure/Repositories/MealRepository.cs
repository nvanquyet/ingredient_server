using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Infrastructure.Data;
using IngredientServer.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class MealRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<Meal>(context, userContextService), IMealRepository
{
    public async Task<IEnumerable<Meal>> GetByDateAsync(string date, int pageNumber = 1, int pageSize = 10)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
        {
            throw new ArgumentException("Invalid date format.");
        }

        return await Context.Set<Meal>()
            .Where(m => m.UserId == AuthenticatedUserId && m.MealDate.Date == parsedDate.Date)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Meal>> GetByDateAsync(DateTime date)
    {
        return await Context.Set<Meal>()
            .Include(m => m.MealFoods)
            .Where(m => m.UserId == AuthenticatedUserId && m.MealDate.Date == date)
            .ToListAsync();
    }

    public async Task<Meal> GetByIdWithFoodsAsync(int mealId)
    {
        var meal = await Context.Set<Meal>()
            .Include(m => m.MealFoods)
            .ThenInclude(mf => mf.Food)
            .Where(m => m.Id == mealId && m.UserId == AuthenticatedUserId)
            .FirstOrDefaultAsync();

        if (meal == null)
        {
            throw new UnauthorizedAccessException("Meal not found or access denied.");
        }

        return meal;
    }
}